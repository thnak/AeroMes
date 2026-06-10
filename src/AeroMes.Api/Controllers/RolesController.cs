using AeroMes.Api.Auth;
using AeroMes.Application.Auth;
using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/roles")]
[Authorize]
public class RolesController(
    RoleManager<IdentityRole> roleManager,
    AppDbContext db,
    IAuditLogger auditLogger) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.RoleRead)]
    [ProducesResponseType<IReadOnlyList<RoleDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var roles = await roleManager.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto(r.Id, r.Name))
            .ToListAsync();

        return Ok(roles);
    }

    [HttpPost]
    [RequirePermission(Permissions.RoleCreate)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<IdentityErrorResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        var result = await roleManager.CreateAsync(new IdentityRole(request.Name));
        if (!result.Succeeded)
            return BadRequest(new IdentityErrorResult(result.Errors.Select(e => e.Description)));

        return CreatedAtAction(nameof(GetAll), null);
    }

    [HttpDelete("{id}")]
    [RequirePermission(Permissions.RoleDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<IdentityErrorResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is null) return NotFound();

        var result = await roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            return BadRequest(new IdentityErrorResult(result.Errors.Select(e => e.Description)));

        return NoContent();
    }

    [HttpGet("{id}/permissions")]
    [RequirePermission(Permissions.RoleRead)]
    [ProducesResponseType<IReadOnlyList<PermissionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPermissions(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is null) return NotFound();

        var perms = await db.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == id)
            .Join(db.Permissions, rp => rp.PermissionId, p => p.PermissionId,
                (_, p) => new PermissionDto(p.PermissionId, p.PermissionCode, p.Resource, p.Action, p.Description))
            .OrderBy(p => p.PermissionCode)
            .ToListAsync();

        return Ok(perms);
    }

    [HttpPut("{id}/permissions")]
    [RequirePermission(Permissions.RoleManagePermissions)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetPermissions(string id, [FromBody] SetPermissionsRequest request)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is null) return NotFound();

        var existing = await db.RolePermissions
            .Where(rp => rp.RoleId == id)
            .ToListAsync();

        db.RolePermissions.RemoveRange(existing);

        var permIds = await db.Permissions
            .Where(p => request.PermissionCodes.Contains(p.PermissionCode))
            .Select(p => p.PermissionId)
            .ToListAsync();

        db.RolePermissions.AddRange(
            permIds.Select(pid => AeroMes.Domain.Auth.RolePermission.Create(id, pid)));

        await db.SaveChangesAsync();

        auditLogger.Log(new SecurityAuditEvent
        {
            EventType = AuditEventTypes.RolePermissionChanged,
            ActorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            ActorType = "USER",
            TargetType = "Role", TargetId = id,
            NewValues = string.Join(",", request.PermissionCodes),
        });
        return NoContent();
    }
}

[ApiController]
[Route("api/v1/auth/permissions")]
[Authorize]
public class PermissionsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.PermissionRead)]
    [ProducesResponseType<IReadOnlyList<PermissionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var perms = await db.Permissions
            .AsNoTracking()
            .OrderBy(p => p.PermissionCode)
            .Select(p => new PermissionDto(p.PermissionId, p.PermissionCode, p.Resource, p.Action, p.Description))
            .ToListAsync();

        return Ok(perms);
    }
}

public record CreateRoleRequest(string Name);
public record SetPermissionsRequest(string[] PermissionCodes);
public record RoleDto(string Id, string? Name);
public record PermissionDto(int PermissionId, string PermissionCode, string Resource, string Action, string? Description);
