using AeroMes.Api.Auth;
using AeroMes.Application.Auth.Permissions;
using AeroMes.Application.Auth.Permissions.Queries.GetAllPermissions;
using AeroMes.Application.Auth.Roles.Commands.SetRolePermissions;
using AeroMes.Application.Auth.Roles.Queries.GetRolePermissions;
using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/roles")]
[Authorize]
public class RolesController(
    RoleManager<IdentityRole> roleManager,
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
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
        var result = await queryMediator.QueryAsync(new GetRolePermissionsQuery(id));
        return Ok(result);
    }

    [HttpPut("{id}/permissions")]
    [RequirePermission(Permissions.RoleManagePermissions)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetPermissions(string id, [FromBody] SetPermissionsRequest request)
    {
        var actorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await commandMediator.SendAsync(new SetRolePermissionsCommand(id, request.PermissionCodes, actorId));
        return NoContent();
    }
}

[ApiController]
[Route("api/v1/auth/permissions")]
[Authorize]
public class PermissionsController(IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.PermissionRead)]
    [ProducesResponseType<IReadOnlyList<PermissionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var result = await queryMediator.QueryAsync(new GetAllPermissionsQuery());
        return Ok(result);
    }
}

public record CreateRoleRequest(string Name);
public record SetPermissionsRequest(string[] PermissionCodes);
public record RoleDto(string Id, string? Name);
