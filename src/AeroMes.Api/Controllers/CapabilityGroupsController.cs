using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Master.CapabilityGroupMembers.Commands.AssignMember;
using AeroMes.Application.Master.CapabilityGroupMembers.Commands.UnassignMember;
using AeroMes.Application.Master.CapabilityGroupMembers.Queries.GetMembers;
using AeroMes.Application.Master.CapabilityGroups.Commands.CreateCapabilityGroup;
using AeroMes.Application.Master.CapabilityGroups.Commands.DeleteCapabilityGroup;
using AeroMes.Application.Master.CapabilityGroups.Commands.UpdateCapabilityGroup;
using AeroMes.Application.Master.CapabilityGroups.Queries.GetCapabilityGroups;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/capability-groups")]
[Authorize]
public class CapabilityGroupsController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<CapabilityGroupDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetCapabilityGroupsQuery(activeOnly), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<CapabilityGroupCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateCapabilityGroupRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new CreateCapabilityGroupCommand(req.Code, req.Name, req.Description), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetAll), null, new CapabilityGroupCreatedResult(result.Value!));
    }

    [HttpPut("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateCapabilityGroupRequest req, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await commandMediator.SendAsync(new UpdateCapabilityGroupCommand(code, req.Name, req.Description, req.IsActive, userId), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(string code, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new DeleteCapabilityGroupCommand(code, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // --- Members ---

    [HttpGet("{code}/members")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<CapabilityGroupMemberDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembers(string code, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetCapabilityGroupMembersQuery(code), null, ct));

    [HttpPost("{code}/members")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<MemberAssignedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AssignMember(string code, [FromBody] AssignMemberRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AssignCapabilityGroupMemberCommand(code, req.ResourceType, req.ResourceId, User.Identity?.Name),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetMembers), new { code }, new MemberAssignedResult(result.Value!));
    }

    [HttpDelete("{code}/members/{memberId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnassignMember(string code, int memberId, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UnassignCapabilityGroupMemberCommand(code, memberId, User.FindFirst(ClaimTypes.NameIdentifier)?.Value),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateCapabilityGroupRequest(string Code, string Name, string? Description);
public record UpdateCapabilityGroupRequest(string Name, string? Description, bool IsActive);
public record CapabilityGroupCreatedResult(string GroupCode);
public record AssignMemberRequest(CapabilityResourceType ResourceType, string ResourceId);
public record MemberAssignedResult(int MemberId);
