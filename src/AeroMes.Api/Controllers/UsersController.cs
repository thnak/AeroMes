using AeroMes.Api.Auth;
using AeroMes.Application.Auth;
using AeroMes.Application.Interfaces;
using AeroMes.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IPermissionService permissionService,
    IAuditLogger auditLogger) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.UserRead)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? role,
        [FromQuery] string? department,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(u => u.Department == department);

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(role))
        {
            var roleObj = await roleManager.FindByNameAsync(role);
            if (roleObj is null) return Ok(new { items = Array.Empty<object>(), total = 0 });

            var userIdsInRole = await userManager.GetUsersInRoleAsync(role);
            var ids = userIdsInRole.Select(u => u.Id).ToHashSet();
            query = query.Where(u => ids.Contains(u.Id));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserSummaryDto(
                u.Id, u.Email!, u.FullName, u.Department,
                u.IsActive, u.LastLoginAt, u.PreferredLanguage))
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    [RequirePermission(Permissions.UserRead)]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new UserDetailDto(
            user.Id, user.Email!, user.FullName, user.Department,
            user.EmployeeCode, user.IsActive, user.ForcePasswordChange,
            user.LastLoginAt, user.PreferredLanguage, user.AvatarUrl,
            user.DefaultWorkCenterId, [.. roles]));
    }

    [HttpPost]
    [RequirePermission(Permissions.UserCreate)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            Department = request.Department,
            EmployeeCode = request.EmployeeCode,
            EmailConfirmed = true,
            ForcePasswordChange = true,
            IsActive = true,
        };

        var tempPassword = GenerateTempPassword();
        var result = await userManager.CreateAsync(user, tempPassword);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        if (request.Roles is { Length: > 0 })
            await userManager.AddToRolesAsync(user, request.Roles);

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.UserCreated,
            ActorId = userManager.GetUserId(User), ActorType = "USER",
            TargetType = "User", TargetId = user.Id,
            NewValues = $"{{\"email\":\"{user.Email}\",\"roles\":\"{string.Join(",", request.Roles ?? [])}\"}}",
        });

        return CreatedAtAction(nameof(GetById), new { id = user.Id },
            new { user.Id, tempPassword });
    }

    [HttpPut("{id}")]
    [RequirePermission(Permissions.UserUpdate)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest request)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        user.FullName = request.FullName ?? user.FullName;
        user.Department = request.Department ?? user.Department;
        user.EmployeeCode = request.EmployeeCode ?? user.EmployeeCode;
        user.PreferredLanguage = request.PreferredLanguage ?? user.PreferredLanguage;
        user.AvatarUrl = request.AvatarUrl ?? user.AvatarUrl;
        user.DefaultWorkCenterId = request.DefaultWorkCenterId ?? user.DefaultWorkCenterId;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return NoContent();
    }

    [HttpDelete("{id}")]
    [RequirePermission(Permissions.UserDelete)]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        if (await IsLastSystemAdmin(user))
            return BadRequest(new { message = "Cannot deactivate the last SYSTEM_ADMIN account." });

        user.IsActive = false;
        await userManager.UpdateAsync(user);
        await userManager.UpdateSecurityStampAsync(user); // revokes all active sessions

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.UserDeactivated,
            ActorId = userManager.GetUserId(User), ActorType = "USER",
            TargetType = "User", TargetId = id,
        });
        return NoContent();
    }

    [HttpPost("{id}/activate")]
    [RequirePermission(Permissions.UserUpdate)]
    public async Task<IActionResult> Activate(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        user.IsActive = true;
        await userManager.UpdateAsync(user);
        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.UserActivated,
            ActorId = userManager.GetUserId(User), ActorType = "USER",
            TargetType = "User", TargetId = id,
        });
        return NoContent();
    }

    [HttpGet("{id}/roles")]
    [RequirePermission(Permissions.UserRead)]
    public async Task<IActionResult> GetRoles(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var roles = await userManager.GetRolesAsync(user);
        return Ok(roles);
    }

    [HttpPost("{id}/roles")]
    [RequirePermission(Permissions.UserManageRoles)]
    public async Task<IActionResult> SetRoles(string id, [FromBody] SetRolesRequest request)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        if (await IsLastSystemAdmin(user) &&
            !request.RoleNames.Contains(AppRoles.SystemAdmin, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "Cannot remove SYSTEM_ADMIN from the last admin account." });

        var current = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, current);
        await userManager.AddToRolesAsync(user, request.RoleNames);
        await permissionService.InvalidateCacheAsync(user.Id);

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.RoleAssigned,
            ActorId = userManager.GetUserId(User), ActorType = "USER",
            TargetType = "User", TargetId = id,
            OldValues = string.Join(",", current),
            NewValues = string.Join(",", request.RoleNames),
        });
        return NoContent();
    }

    [HttpPost("{id}/reset-password")]
    [RequirePermission(Permissions.UserResetPassword)]
    public async Task<IActionResult> ResetPassword(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var tempPassword = GenerateTempPassword();
        var result = await userManager.ResetPasswordAsync(user, token, tempPassword);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        user.ForcePasswordChange = true;
        await userManager.UpdateAsync(user);

        return Ok(new { tempPassword });
    }

    private async Task<bool> IsLastSystemAdmin(ApplicationUser user)
    {
        if (!await userManager.IsInRoleAsync(user, AppRoles.SystemAdmin)) return false;
        var admins = await userManager.GetUsersInRoleAsync(AppRoles.SystemAdmin);
        return admins.Count(a => a.IsActive) <= 1;
    }

    private static string GenerateTempPassword()
    {
        const string chars = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$";
        var random = new Random();
        return new string([.. Enumerable.Range(0, 12).Select(_ => chars[random.Next(chars.Length)])]);
    }
}

public record UserSummaryDto(
    string Id, string Email, string FullName, string? Department,
    bool IsActive, DateTimeOffset? LastLoginAt, string? PreferredLanguage);

public record UserDetailDto(
    string Id, string Email, string FullName, string? Department,
    string? EmployeeCode, bool IsActive, bool ForcePasswordChange,
    DateTimeOffset? LastLoginAt, string? PreferredLanguage, string? AvatarUrl,
    int? DefaultWorkCenterId, string[] Roles);

public record CreateUserRequest(
    string Email, string FullName, string? Department,
    string? EmployeeCode, string[]? Roles);

public record UpdateUserRequest(
    string? FullName, string? Department, string? EmployeeCode,
    string? PreferredLanguage, string? AvatarUrl, int? DefaultWorkCenterId);

public record SetRolesRequest(string[] RoleNames);
