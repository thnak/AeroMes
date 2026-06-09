using AeroMes.Application.Auth;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using AeroMes.Infrastructure.Data;
using AeroMes.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    AppDbContext db,
    IAuditLogger auditLogger,
    IConfiguration configuration) : ControllerBase
{
    private const string RefreshTokenCookie = "refresh_token";
    private const string RefreshCookiePath = "/api/v1/auth";

    [HttpPost("login")]
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
                new { message = "Account locked due to too many failed attempts. Try again later." });
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
            return Unauthorized(new { message = "Invalid email or password." });
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
            return Unauthorized(new { message = "Account is disabled." });
        }

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.CreateToken(user.Id, user.Email!, roles, user.DefaultWorkCenterId);

        var (rawRefresh, refreshEntity) = CreateRefreshToken(user.Id, ua, ip);
        db.RefreshTokens.Add(refreshEntity);

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await userManager.UpdateAsync(user);
        await db.SaveChangesAsync();

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
    public async Task<IActionResult> Refresh()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var raw = Request.Cookies[RefreshTokenCookie];
        if (raw is null) return Unauthorized(new { message = "No refresh token." });

        var hash = HashToken(raw);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);

        if (stored is null)
            return Unauthorized(new { message = "Invalid refresh token." });

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
            return Unauthorized(new { message = "Refresh token reuse detected. All sessions revoked." });
        }

        if (stored.ExpiresAt <= DateTime.UtcNow)
        {
            DeleteRefreshCookie();
            return Unauthorized(new { message = "Refresh token expired." });
        }

        var user = await userManager.FindByIdAsync(stored.UserId);
        if (user is null || !user.IsActive)
            return Unauthorized(new { message = "Account disabled." });

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.CreateToken(user.Id, user.Email!, roles, user.DefaultWorkCenterId);

        var (rawRefresh, newEntity) = CreateRefreshToken(user.Id,
            Request.Headers.UserAgent.ToString(), ip, stored.FamilyId);
        db.RefreshTokens.Add(newEntity);
        await db.SaveChangesAsync();

        stored.Revoke(newEntity.TokenId);
        await db.SaveChangesAsync();

        SetRefreshCookie(rawRefresh, newEntity.ExpiresAt);

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.AuthTokenRefresh,
            ActorId = user.Id, ActorType = "USER", ActorIp = ip,
        });

        return Ok(new { accessToken, tokenType = "Bearer", expiresIn = AccessTokenLifetimeSeconds });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var raw = Request.Cookies[RefreshTokenCookie];
        if (raw is not null)
        {
            var hash = HashToken(raw);
            var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
            if (stored is { RevokedAt: null })
            {
                stored.Revoke();
                await db.SaveChangesAsync();
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
    public async Task<IActionResult> LogoutAll()
    {
        var userId = userManager.GetUserId(User)!;
        var tokens = await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();

        foreach (var t in tokens) t.Revoke();
        await db.SaveChangesAsync();

        await signInManager.SignOutAsync();
        DeleteRefreshCookie();
        return NoContent();
    }

    [HttpGet("sessions")]
    [Authorize]
    public async Task<IActionResult> GetSessions()
    {
        var userId = userManager.GetUserId(User)!;
        var currentHash = Request.Cookies.TryGetValue(RefreshTokenCookie, out var raw)
            ? HashToken(raw) : null;

        var sessions = await db.RefreshTokens
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new SessionDto(
                t.TokenId, t.DeviceInfo, t.IpAddress,
                t.CreatedAt, t.ExpiresAt,
                t.TokenHash == currentHash))
            .ToListAsync();

        return Ok(sessions);
    }

    [HttpDelete("sessions/{tokenId:long}")]
    [Authorize]
    public async Task<IActionResult> RevokeSession(long tokenId)
    {
        var userId = userManager.GetUserId(User)!;
        var token = await db.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenId == tokenId && t.UserId == userId);

        if (token is null) return NotFound();
        token.Revoke();
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var roles = await userManager.GetRolesAsync(user);
        return Ok(new
        {
            user.Id,
            user.Email,
            user.FullName,
            user.Department,
            user.PreferredLanguage,
            user.AvatarUrl,
            user.ForcePasswordChange,
            user.DefaultWorkCenterId,
            Roles = roles,
        });
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateMeRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        user.FullName = request.FullName ?? user.FullName;
        user.PreferredLanguage = request.PreferredLanguage ?? user.PreferredLanguage;
        user.AvatarUrl = request.AvatarUrl ?? user.AvatarUrl;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return NoContent();
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

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

    private async Task RevokeFamily(Guid familyId)
    {
        var family = await db.RefreshTokens
            .Where(t => t.FamilyId == familyId && t.RevokedAt == null)
            .ToListAsync();
        foreach (var t in family) t.Revoke();
        await db.SaveChangesAsync();
    }
}

public record LoginRequest(string Email, string Password);
public record LoginResponse(
    string AccessToken, string TokenType, int ExpiresIn,
    string Email, string FullName, string[] Roles, bool ForcePasswordChange);
public record UpdateMeRequest(string? FullName, string? PreferredLanguage, string? AvatarUrl);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record SessionDto(long TokenId, string? DeviceInfo, string? IpAddress,
    DateTime CreatedAt, DateTime ExpiresAt, bool IsCurrent);
