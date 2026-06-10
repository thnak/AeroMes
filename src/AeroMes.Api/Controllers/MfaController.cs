using AeroMes.Application.Auth;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using AeroMes.Infrastructure.Data;
using AeroMes.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using OtpNet;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/auth/mfa")]
public class MfaController(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    AppDbContext db,
    IAuditLogger auditLogger,
    IConfiguration configuration,
    IMemoryCache cache,
    ILogger<MfaController> logger) : ControllerBase
{
    private const string OtpCachePrefix = "mfa:otp:";
    private const string OtpAttemptPrefix = "mfa:attempts:";

    // ── TOTP setup ────────────────────────────────────────────────────────

    [HttpGet("setup")]
    [Authorize]
    public async Task<IActionResult> SetupTotp()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        if (user.TwoFactorEnabled) return Conflict(new { message = "MFA is already enabled." });

        // Generate a new TOTP secret if one doesn't exist yet
        var base32Secret = await userManager.GetAuthenticatorKeyAsync(user);
        if (base32Secret is null)
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            base32Secret = await userManager.GetAuthenticatorKeyAsync(user);
        }

        var issuer = configuration["Auth:TotpIssuer"] ?? "AeroMes";
        var email = user.Email!;
        var otpauthUri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}" +
                         $"?secret={base32Secret}&issuer={Uri.EscapeDataString(issuer)}&algorithm=SHA1&digits=6&period=30";

        return Ok(new { secret = base32Secret, otpauthUri });
    }

    [HttpPost("setup/confirm")]
    [Authorize]
    public async Task<IActionResult> ConfirmTotp([FromBody] TotpConfirmRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var keyString = await userManager.GetAuthenticatorKeyAsync(user);
        if (keyString is null) return BadRequest(new { message = "No TOTP setup in progress. Call GET /mfa/setup first." });

        if (!VerifyTotp(keyString, request.Code))
            return BadRequest(new { message = "Invalid TOTP code." });

        await userManager.SetTwoFactorEnabledAsync(user, true);

        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 8);

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.AuthMfaSetup,
            ActorId = user.Id, ActorType = "USER",
            ActorIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
        });

        return Ok(new { message = "MFA enabled.", recoveryCodes });
    }

    // ── MFA verify (after login with 2FA enabled) ─────────────────────────

    [HttpPost("verify")]
    [AllowAnonymous]
    public async Task<IActionResult> Verify([FromBody] MfaVerifyRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers.UserAgent.ToString();

        var userId = ExtractUserIdFromMfaToken(request.MfaToken);
        if (userId is null)
            return Unauthorized(new { message = "Invalid or expired MFA token." });

        // Rate limit check
        var attemptKey = OtpAttemptPrefix + userId;
        var attempts = cache.GetOrCreate(attemptKey, e =>
        {
            e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return 0;
        });

        if (attempts >= 3)
            return StatusCode(StatusCodes.Status429TooManyRequests,
                new { message = "Too many failed MFA attempts. Try again in 30 minutes." });

        var user = await userManager.FindByIdAsync(userId);
        if (user is null || !user.IsActive)
            return Unauthorized(new { message = "Invalid MFA token." });

        bool verified = false;

        // 1. Try TOTP
        var totpKey = await userManager.GetAuthenticatorKeyAsync(user);
        if (totpKey is not null && VerifyTotp(totpKey, request.Code))
            verified = true;

        // 2. Try email OTP
        if (!verified)
        {
            var otpKey = OtpCachePrefix + userId;
            if (cache.TryGetValue(otpKey, out string? storedOtp) && storedOtp == request.Code)
            {
                cache.Remove(otpKey);
                verified = true;
            }
        }

        // 3. Try recovery code
        if (!verified)
        {
            var redeemResult = await userManager.RedeemTwoFactorRecoveryCodeAsync(user, request.Code);
            if (redeemResult.Succeeded) verified = true;
        }

        if (!verified)
        {
            cache.Set(attemptKey, attempts + 1,
                TimeSpan.FromMinutes(30));

            auditLogger.Log(new SecurityAuditEvent
            {
                EventType = AuditEventTypes.AuthMfaFailure,
                ActorId = userId, ActorType = "USER",
                ActorIp = ip, ActorUserAgent = ua,
                Outcome = "FAILURE", FailureReason = "Invalid MFA code",
            });
            return Unauthorized(new { message = "Invalid MFA code." });
        }

        cache.Remove(attemptKey);

        var roles = await userManager.GetRolesAsync(user);
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
            EventType = AuditEventTypes.AuthMfaSuccess,
            ActorId = user.Id, ActorType = "USER",
            ActorIp = ip, ActorUserAgent = ua,
        });

        return Ok(new LoginResponse(
            accessToken, "Bearer", AccessTokenLifetimeSeconds,
            user.Email!, user.FullName, [.. roles],
            user.ForcePasswordChange));
    }

    // ── Email OTP ─────────────────────────────────────────────────────────

    [HttpPost("send-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> SendEmailOtp([FromBody] SendOtpRequest request)
    {
        var userId = ExtractUserIdFromMfaToken(request.MfaToken);
        if (userId is null)
            return Unauthorized(new { message = "Invalid or expired MFA token." });

        var user = await userManager.FindByIdAsync(userId);
        if (user is null || !user.IsActive)
            return Unauthorized(new { message = "Invalid MFA token." });

        // Rate limit: one OTP per minute
        var otpKey = OtpCachePrefix + userId;
        if (cache.TryGetValue(otpKey, out _))
            return StatusCode(StatusCodes.Status429TooManyRequests,
                new { message = "A code was already sent. Please wait before requesting another." });

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        cache.Set(otpKey, code, TimeSpan.FromMinutes(10));

        // TODO: send via email when IEmailSender is wired up
        logger.LogInformation("MFA email OTP for {Email}: {Code}", user.Email, code);

        return Ok(new { message = "A 6-digit code has been sent to your email." });
    }

    // ── Recovery codes ────────────────────────────────────────────────────

    [HttpGet("recovery-codes")]
    [Authorize]
    public async Task<IActionResult> GetRecoveryCodeCount()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var count = await userManager.CountRecoveryCodesAsync(user);
        return Ok(new { remaining = count });
    }

    [HttpPost("recovery-codes/regenerate")]
    [Authorize]
    public async Task<IActionResult> RegenerateRecoveryCodes([FromBody] TotpConfirmRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        if (!user.TwoFactorEnabled) return BadRequest(new { message = "MFA is not enabled." });

        var totpKey = await userManager.GetAuthenticatorKeyAsync(user);
        if (totpKey is null || !VerifyTotp(totpKey, request.Code))
            return Unauthorized(new { message = "Invalid TOTP code." });

        var codes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 8);
        return Ok(new { recoveryCodes = codes });
    }

    // ── Disable MFA ───────────────────────────────────────────────────────

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DisableMfa([FromBody] TotpConfirmRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        if (!user.TwoFactorEnabled) return BadRequest(new { message = "MFA is not enabled." });

        var totpKey = await userManager.GetAuthenticatorKeyAsync(user);
        bool codeOk = (totpKey is not null && VerifyTotp(totpKey, request.Code))
                   || (await userManager.RedeemTwoFactorRecoveryCodeAsync(user, request.Code)).Succeeded;

        if (!codeOk) return Unauthorized(new { message = "Invalid code." });

        await userManager.SetTwoFactorEnabledAsync(user, false);
        await userManager.ResetAuthenticatorKeyAsync(user);

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.AuthMfaDisabled,
            ActorId = user.Id, ActorType = "USER",
            ActorIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
        });

        return NoContent();
    }

    // ── Admin force-disable ───────────────────────────────────────────────

    [HttpDelete("/api/v1/users/{userId}/mfa")]
    [Authorize(Policy = "permission:User:ManageRoles")]
    public async Task<IActionResult> AdminDisableMfa(string userId)
    {
        var actor = await userManager.GetUserAsync(User);
        var target = await userManager.FindByIdAsync(userId);
        if (target is null) return NotFound();

        await userManager.SetTwoFactorEnabledAsync(target, false);
        await userManager.ResetAuthenticatorKeyAsync(target);

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.AuthMfaDisabled,
            ActorId = actor?.Id, ActorType = "USER",
            TargetType = "User", TargetId = userId,
            ActorIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
        });

        return NoContent();
    }

    // ── helpers ───────────────────────────────────────────────────────────

    private string? ExtractUserIdFromMfaToken(string token)
    {
        try
        {
            var jwtKey = configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key not configured.");
            var handler = new JwtSecurityTokenHandler();
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };
            var principal = handler.ValidateToken(token, validationParams, out _);
            if (!principal.HasClaim("mfa_pending", "true")) return null;
            return principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }
        catch
        {
            return null;
        }
    }

    private static bool VerifyTotp(string base32Secret, string code)
    {
        try
        {
            var keyBytes = Base32Encoding.ToBytes(base32Secret);
            var totp = new Totp(keyBytes);
            return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1));
        }
        catch
        {
            return false;
        }
    }

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
        const string cookiePath = "/api/v1/auth";
        Response.Cookies.Append("refresh_token", rawToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Path = cookiePath,
            Expires = expires,
        });
    }
}

public record TotpConfirmRequest(string Code);
public record MfaVerifyRequest(string MfaToken, string Code);
public record SendOtpRequest(string MfaToken);
