using AeroMes.Application.Auth;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using AeroMes.Infrastructure.Data;
using AeroMes.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/auth/passkey")]
public class PasskeyController(
    UserManager<ApplicationUser> userManager,
    IPasskeyHandler<ApplicationUser> passkeyHandler,
    ITokenService tokenService,
    AppDbContext db,
    IAuditLogger auditLogger,
    IConfiguration configuration) : ControllerBase
{
    private const string RefreshTokenCookie = "refresh_token";
    private const string RefreshCookiePath = "/api/v1/auth";

    // ── Registration ──────────────────────────────────────────────────────

    /// <summary>
    /// Returns WebAuthn creation options for registering a new passkey.
    /// The returned <c>attestationState</c> must be included in the subsequent /register call.
    /// </summary>
    [HttpGet("attestation-options")]
    [Authorize]
    public async Task<IActionResult> GetAttestationOptions()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var userEntity = new PasskeyUserEntity
        {
            Id = user.Id,
            Name = user.Email!,
            DisplayName = user.FullName ?? user.Email!,
        };

        var result = await passkeyHandler.MakeCreationOptionsAsync(userEntity, HttpContext);
        return Ok(new
        {
            creationOptionsJson = result.CreationOptionsJson,
            attestationState = result.AttestationState,
        });
    }

    /// <summary>
    /// Completes passkey registration.
    /// </summary>
    [HttpPost("register")]
    [Authorize]
    public async Task<IActionResult> Register([FromBody] PasskeyRegisterRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var context = new PasskeyAttestationContext
        {
            HttpContext = HttpContext,
            CredentialJson = request.CredentialJson,
            AttestationState = request.AttestationState,
        };

        var result = await passkeyHandler.PerformAttestationAsync(context);
        if (!result.Succeeded)
            return BadRequest(new { message = "Passkey registration failed.", detail = result.Failure?.Message });

        // Assign a name to the passkey (device/browser name from request or generated)
        var passkey = result.Passkey!;
        passkey.Name = request.Name ?? "Passkey";
        await userManager.AddOrUpdatePasskeyAsync(user, passkey);

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.AuthPasskeyRegistered,
            ActorId = user.Id, ActorType = "USER",
            ActorIp = ip,
            TargetType = "Passkey",
            TargetId = Convert.ToBase64String(result.Passkey!.CredentialId),
        });

        return Ok(new { message = "Passkey registered successfully." });
    }

    // ── Login ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns WebAuthn request options for passkey login (discoverable credentials).
    /// The returned <c>assertionState</c> must be included in the subsequent /login call.
    /// </summary>
    [HttpGet("assertion-options")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAssertionOptions()
    {
        // null user = discoverable credentials (any passkey registered with this RP)
        var result = await passkeyHandler.MakeRequestOptionsAsync(null!, HttpContext);
        return Ok(new
        {
            requestOptionsJson = result.RequestOptionsJson,
            assertionState = result.AssertionState,
        });
    }

    /// <summary>
    /// Completes passkey authentication and issues access + refresh tokens.
    /// Passkey login counts as MFA-verified (the authenticator provides the second factor).
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] PasskeyLoginRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers.UserAgent.ToString();

        var context = new PasskeyAssertionContext
        {
            HttpContext = HttpContext,
            CredentialJson = request.CredentialJson,
            AssertionState = request.AssertionState,
        };

        var result = await passkeyHandler.PerformAssertionAsync(context);
        if (!result.Succeeded)
        {
            auditLogger.Log(new SecurityAuditEvent
            {
                EventType = AuditEventTypes.AuthLoginFailure,
                ActorIp = ip, ActorUserAgent = ua,
                Outcome = "FAILURE", FailureReason = result.Failure?.Message ?? "Passkey assertion failed",
            });
            return Unauthorized(new { message = "Passkey authentication failed." });
        }

        var user = result.User!;
        if (!user.IsActive)
            return Unauthorized(new { message = "Account is disabled." });

        var roles = await userManager.GetRolesAsync(user);
        // Passkey authentication inherently satisfies MFA — set mfaVerified=true
        var accessToken = tokenService.CreateToken(
            user.Id, user.Email!, roles, user.DefaultWorkCenterId, mfaVerified: true);

        var (rawRefresh, refreshEntity) = CreateRefreshToken(user.Id, ua, ip);
        db.RefreshTokens.Add(refreshEntity);
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await userManager.UpdateAsync(user);
        await db.SaveChangesAsync();

        SetRefreshCookie(rawRefresh, refreshEntity.ExpiresAt);

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.AuthPasskeyLogin,
            ActorId = user.Id, ActorType = "USER",
            ActorIp = ip, ActorUserAgent = ua,
            TargetType = "Passkey",
            TargetId = Convert.ToBase64String(result.Passkey!.CredentialId),
        });

        return Ok(new LoginResponse(
            accessToken, "Bearer", AccessTokenLifetimeSeconds,
            user.Email!, user.FullName, [.. roles],
            user.ForcePasswordChange));
    }

    // ── Management ────────────────────────────────────────────────────────

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMyPasskeys()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var passkeys = await userManager.GetPasskeysAsync(user);
        return Ok(passkeys.Select(p => new PasskeyDto(
            Convert.ToBase64String(p.CredentialId),
            p.Name ?? "Passkey",
            p.CreatedAt,
            p.IsBackedUp,
            p.Transports ?? [])));
    }

    [HttpDelete("{credentialIdBase64}")]
    [Authorize]
    public async Task<IActionResult> RemovePasskey(string credentialIdBase64)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        byte[] credentialId;
        try { credentialId = Convert.FromBase64String(credentialIdBase64); }
        catch { return BadRequest(new { message = "Invalid credential ID format." }); }

        var passkey = await userManager.GetPasskeyAsync(user, credentialId);
        if (passkey is null) return NotFound();

        await userManager.RemovePasskeyAsync(user, credentialId);

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.AuthPasskeyRemoved,
            ActorId = user.Id, ActorType = "USER",
            ActorIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
            TargetType = "Passkey", TargetId = credentialIdBase64,
        });

        return NoContent();
    }

    // ── helpers ───────────────────────────────────────────────────────────

    private int AccessTokenLifetimeSeconds =>
        int.TryParse(configuration["Jwt:ExpiryMinutes"], out var m) ? m * 60 : 900;

    private int RefreshTokenLifetimeDays =>
        int.TryParse(configuration["Jwt:RefreshTokenDays"], out var d) ? d : 7;

    private static string HashToken(string raw)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));

    private (string raw, RefreshToken entity) CreateRefreshToken(
        string userId, string? deviceInfo, string? ip)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hash = HashToken(raw);
        var expires = DateTime.UtcNow.AddDays(RefreshTokenLifetimeDays);
        var entity = RefreshToken.Create(userId, hash, Guid.NewGuid(), expires,
            deviceInfo?.Length > 200 ? deviceInfo[..200] : deviceInfo, ip);
        return (raw, entity);
    }

    private void SetRefreshCookie(string rawToken, DateTime expires)
    {
        Response.Cookies.Append(RefreshTokenCookie, rawToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Path = RefreshCookiePath,
            Expires = expires,
        });
    }
}

public record PasskeyRegisterRequest(string CredentialJson, string AttestationState, string? Name);
public record PasskeyLoginRequest(string CredentialJson, string AssertionState);
public record PasskeyDto(string CredentialId, string Name, DateTimeOffset CreatedAt, bool IsBackedUp, string[] Transports);
