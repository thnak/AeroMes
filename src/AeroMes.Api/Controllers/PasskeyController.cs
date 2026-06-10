using AeroMes.Application.Auth;
using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
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
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork uow,
    IAuditLogger auditLogger,
    IConfiguration configuration) : ControllerBase
{
    private const string RefreshTokenCookie = "refresh_token";
    private const string RefreshCookiePath = "/api/v1/auth";

    // ── Registration ──────────────────────────────────────────────────────

    [HttpGet("attestation-options")]
    [Authorize]
    [ProducesResponseType<AttestationOptionsResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
        return Ok(new AttestationOptionsResult(result.CreationOptionsJson, result.AttestationState!));
    }

    [HttpPost("register")]
    [Authorize]
    [ProducesResponseType<MessageResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<PasskeyErrorResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
            return BadRequest(new PasskeyErrorResult("Passkey registration failed.", result.Failure?.Message));

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

        return Ok(new MessageResult("Passkey registered successfully."));
    }

    // ── Login ─────────────────────────────────────────────────────────────

    [HttpGet("assertion-options")]
    [AllowAnonymous]
    [ProducesResponseType<AssertionOptionsResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssertionOptions()
    {
        var result = await passkeyHandler.MakeRequestOptionsAsync(null!, HttpContext);
        return Ok(new AssertionOptionsResult(result.RequestOptionsJson, result.AssertionState!));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
            return Unauthorized(new MessageResult("Passkey authentication failed."));
        }

        var user = result.User!;
        if (!user.IsActive)
            return Unauthorized(new MessageResult("Account is disabled."));

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.CreateToken(
            user.Id, user.Email!, roles, user.DefaultWorkCenterId, mfaVerified: true);

        var (rawRefresh, refreshEntity) = CreateRefreshToken(user.Id, ua, ip);
        refreshTokens.Add(refreshEntity);
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await userManager.UpdateAsync(user);
        await uow.SaveChangesAsync();

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
    [ProducesResponseType<IReadOnlyList<PasskeyDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
            p.Transports ?? [])).ToList());
    }

    [HttpDelete("{credentialIdBase64}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<MessageResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemovePasskey(string credentialIdBase64)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        byte[] credentialId;
        try { credentialId = Convert.FromBase64String(credentialIdBase64); }
        catch { return BadRequest(new MessageResult("Invalid credential ID format.")); }

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
public record AttestationOptionsResult(string CreationOptionsJson, string AttestationState);
public record AssertionOptionsResult(string RequestOptionsJson, string AssertionState);
public record PasskeyErrorResult(string Message, string? Detail);
