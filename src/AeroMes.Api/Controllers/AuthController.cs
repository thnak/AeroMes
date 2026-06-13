using AeroMes.Application.Auth;
using AeroMes.Application.Auth.Sessions;
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
[Route("api/v1/auth")]
public class AuthController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokens,
    IAuditLogger auditLogger,
    IEmailSender emailSender,
    IUnitOfWork uow,
    IConfiguration configuration) : ControllerBase
{
    private const string RefreshTokenCookie = "refresh_token";
    private const string RefreshCookiePath = "/api/v1/auth";

    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<MfaPendingResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers.UserAgent.ToString();

        var result = await signInManager.PasswordSignInAsync(
            request.Email, request.Password,
            isPersistent: false, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            auditLogger.Log(new SecurityAuditEvent
            {
                EventType = AuditEventTypes.AuthLoginFailure,
                ActorIp = ip, ActorUserAgent = ua,
                TargetType = "User", TargetId = request.Email,
                Outcome = "FAILURE", FailureReason = "Account locked",
            });
            return StatusCode(StatusCodes.Status429TooManyRequests,
                new MessageResult("Account locked due to too many failed attempts. Try again later."));
        }

        if (!result.Succeeded)
        {
            auditLogger.Log(new SecurityAuditEvent
            {
                EventType = AuditEventTypes.AuthLoginFailure,
                ActorIp = ip, ActorUserAgent = ua,
                TargetType = "User", TargetId = request.Email,
                Outcome = "FAILURE", FailureReason = "Invalid credentials",
            });
            return Unauthorized(new MessageResult("Invalid email or password."));
        }

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
        {
            auditLogger.Log(new SecurityAuditEvent
            {
                EventType = AuditEventTypes.AuthLoginFailure,
                ActorIp = ip, ActorUserAgent = ua,
                TargetType = "User", TargetId = request.Email,
                Outcome = "FAILURE", FailureReason = "Account disabled",
            });
            return Unauthorized(new MessageResult("Account is disabled."));
        }

        if (user.TwoFactorEnabled)
        {
            var mfaToken = tokenService.CreateMfaPendingToken(user.Id, user.Email!);
            auditLogger.Log(new SecurityAuditEvent
            {
                EventType = AuditEventTypes.AuthLoginSuccess,
                ActorId = user.Id, ActorType = "USER",
                ActorIp = ip, ActorUserAgent = ua,
            });
            return Ok(new MfaPendingResult(true, mfaToken));
        }

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.CreateToken(user.Id, user.Email!, roles, user.DefaultWorkCenterId, preferredLanguage: user.PreferredLanguage);

        var (rawRefresh, refreshEntity) = CreateRefreshToken(user.Id, ua, ip);
        refreshTokens.Add(refreshEntity);

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await userManager.UpdateAsync(user);
        await uow.SaveChangesAsync();

        SetRefreshCookie(rawRefresh, refreshEntity.ExpiresAt);

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.AuthLoginSuccess,
            ActorId = user.Id, ActorType = "USER",
            ActorIp = ip, ActorUserAgent = ua,
        });

        return Ok(new LoginResponse(
            accessToken, "Bearer", AccessTokenLifetimeSeconds,
            user.Email!, user.FullName, [.. roles],
            user.ForcePasswordChange));
    }

    [HttpPost("refresh")]
    [ProducesResponseType<RefreshTokenResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var raw = Request.Cookies[RefreshTokenCookie];
        if (raw is null) return Unauthorized(new MessageResult("No refresh token."));

        var hash = HashToken(raw);
        var stored = await refreshTokens.GetByHashAsync(hash);

        if (stored is null)
            return Unauthorized(new MessageResult("Invalid refresh token."));

        if (stored.RevokedAt is not null)
        {
            await RevokeFamily(stored.FamilyId);
            auditLogger.Log(new SecurityAuditEvent
            {
                EventType = AuditEventTypes.AuthTokenReuseAttack,
                ActorId = stored.UserId, ActorType = "USER",
                ActorIp = ip,
                Outcome = "FAILURE", FailureReason = "Revoked token reuse — family revoked",
            });
            DeleteRefreshCookie();
            return Unauthorized(new MessageResult("Refresh token reuse detected. All sessions revoked."));
        }

        if (stored.ExpiresAt <= DateTime.UtcNow)
        {
            DeleteRefreshCookie();
            return Unauthorized(new MessageResult("Refresh token expired."));
        }

        var user = await userManager.FindByIdAsync(stored.UserId);
        if (user is null || !user.IsActive)
            return Unauthorized(new MessageResult("Account disabled."));

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.CreateToken(user.Id, user.Email!, roles, user.DefaultWorkCenterId, preferredLanguage: user.PreferredLanguage);

        var (rawRefresh, newEntity) = CreateRefreshToken(user.Id,
            Request.Headers.UserAgent.ToString(), ip, stored.FamilyId);
        refreshTokens.Add(newEntity);
        await uow.SaveChangesAsync();

        stored.Revoke(newEntity.TokenId);
        await uow.SaveChangesAsync();

        SetRefreshCookie(rawRefresh, newEntity.ExpiresAt);

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.AuthTokenRefresh,
            ActorId = user.Id, ActorType = "USER", ActorIp = ip,
        });

        return Ok(new RefreshTokenResult(accessToken, "Bearer", AccessTokenLifetimeSeconds));
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var raw = Request.Cookies[RefreshTokenCookie];
        if (raw is not null)
        {
            var hash = HashToken(raw);
            var stored = await refreshTokens.GetByHashAsync(hash);
            if (stored is { RevokedAt: null })
            {
                stored.Revoke();
                await uow.SaveChangesAsync();
            }
        }

        var userId = userManager.GetUserId(User);
        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.AuthLogout,
            ActorId = userId, ActorType = "USER",
            ActorIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
        });

        await signInManager.SignOutAsync();
        DeleteRefreshCookie();
        return NoContent();
    }

    [HttpPost("logout-all")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAll()
    {
        var userId = userManager.GetUserId(User)!;
        var tokens = await refreshTokens.GetActiveByUserIdAsync(userId);

        foreach (var t in tokens) t.Revoke();
        await uow.SaveChangesAsync();

        await signInManager.SignOutAsync();
        DeleteRefreshCookie();
        return NoContent();
    }

    [HttpGet("sessions")]
    [Authorize]
    [ProducesResponseType<IReadOnlyList<SessionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSessions()
    {
        var userId = userManager.GetUserId(User)!;
        var currentHash = Request.Cookies.TryGetValue(RefreshTokenCookie, out var raw)
            ? HashToken(raw) : null;

        var rawTokens = await refreshTokens.GetActiveByUserIdAsync(userId);
        var sessions = rawTokens
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new SessionDto(
                t.TokenId, t.DeviceInfo, t.IpAddress,
                t.CreatedAt, t.ExpiresAt,
                t.TokenHash == currentHash))
            .ToList();

        return Ok(sessions);
    }

    [HttpDelete("sessions/{tokenId:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeSession(long tokenId)
    {
        var userId = userManager.GetUserId(User)!;
        var token = await refreshTokens.GetActiveByTokenIdAndUserAsync(tokenId, userId);

        if (token is null) return NotFound();
        token.Revoke();
        await uow.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<UserProfileResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var roles = await userManager.GetRolesAsync(user);
        return Ok(new UserProfileResult(
            user.Id, user.Email, user.FullName, user.Department,
            user.PreferredLanguage, user.AvatarUrl, user.ForcePasswordChange,
            user.DefaultWorkCenterId, [.. roles]));
    }

    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<IdentityErrorResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateMeRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        user.FullName = request.FullName ?? user.FullName;
        user.PreferredLanguage = request.PreferredLanguage ?? user.PreferredLanguage;
        user.AvatarUrl = request.AvatarUrl ?? user.AvatarUrl;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(new IdentityErrorResult(result.Errors.Select(e => e.Description)));

        return NoContent();
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<IdentityErrorResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new IdentityErrorResult(result.Errors.Select(e => e.Description)));

        user.ForcePasswordChange = false;
        await userManager.UpdateAsync(user);
        await signInManager.RefreshSignInAsync(user);

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.AuthPasswordChanged,
            ActorId = user.Id, ActorType = "USER",
            ActorIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
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
        string userId, string? deviceInfo, string? ip, Guid? familyId = null)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hash = HashToken(raw);
        var expires = DateTime.UtcNow.AddDays(RefreshTokenLifetimeDays);
        var entity = RefreshToken.Create(userId, hash, familyId ?? Guid.NewGuid(), expires,
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

    private void DeleteRefreshCookie()
        => Response.Cookies.Delete(RefreshTokenCookie, new CookieOptions { Path = RefreshCookiePath });

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType<MessageResult>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is { IsActive: true })
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var frontendUrl = configuration["App:FrontendUrl"] ?? "http://localhost:5173";
            var resetLink = $"{frontendUrl}/auth/reset-password" +
                            $"?email={Uri.EscapeDataString(user.Email!)}" +
                            $"&token={Uri.EscapeDataString(token)}";
            await emailSender.SendAsync(
                user.Email!,
                "Reset your AeroMes password",
                $"<p>Click the link below to reset your password (valid for 1 hour):</p><p><a href='{resetLink}'>{resetLink}</a></p>",
                ct);
        }
        return Ok(new MessageResult("If that email is registered, a reset link has been sent."));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType<MessageResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<IdentityErrorResult>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return BadRequest(new MessageResult("Invalid or expired reset token."));

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new IdentityErrorResult(result.Errors.Select(e => e.Description)));

        user.ForcePasswordChange = false;
        await userManager.UpdateAsync(user);

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.AuthPasswordChanged,
            ActorId = user.Id, ActorType = "USER",
            ActorIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Outcome = "SUCCESS",
        });

        return Ok(new MessageResult("Password reset successfully."));
    }

    private async Task RevokeFamily(Guid familyId)
    {
        var family = await refreshTokens.GetActiveFamilyAsync(familyId);
        foreach (var t in family) t.Revoke();
        await uow.SaveChangesAsync();
    }
}

public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);
public record LoginRequest(string Email, string Password);
public record LoginResponse(
    string AccessToken, string TokenType, int ExpiresIn,
    string Email, string FullName, string[] Roles, bool ForcePasswordChange);
public record MfaPendingResult(bool RequiresMfa, string MfaToken);
public record RefreshTokenResult(string AccessToken, string TokenType, int ExpiresIn);
public record UserProfileResult(
    string Id, string? Email, string? FullName, string? Department,
    string? PreferredLanguage, string? AvatarUrl, bool ForcePasswordChange,
    int? DefaultWorkCenterId, string[] Roles);
public record UpdateMeRequest(string? FullName, string? PreferredLanguage, string? AvatarUrl);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
