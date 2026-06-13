using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Master.ProductionTeams.Commands.AddTeamMember;
using AeroMes.Application.Master.ProductionTeams.Commands.CreateProductionTeam;
using AeroMes.Application.Master.ProductionTeams.Commands.DeleteProductionTeam;
using AeroMes.Application.Master.ProductionTeams.Commands.DuplicateProductionTeam;
using AeroMes.Application.Master.ProductionTeams.Commands.RemoveTeamMember;
using AeroMes.Application.Master.ProductionTeams.Commands.UpdateProductionTeam;
using AeroMes.Application.Master.ProductionTeams.Queries.GetProductionTeamByCode;
using AeroMes.Application.Master.ProductionTeams.Queries.GetProductionTeams;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/production-teams")]
[Authorize]
public class ProductionTeamsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<ProductionTeamDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool activeOnly = true,
        [FromQuery] string? search = null,
        [FromQuery] int? orgUnitId = null,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetProductionTeamsQuery(activeOnly, search, orgUnitId), null, ct));

    [HttpGet("{code}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<ProductionTeamDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetProductionTeamByCodeQuery(code), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<ProductionTeamCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateProductionTeamRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateProductionTeamCommand(
                req.Code, req.Name, req.OrgUnitId,
                req.StandardLaborQuantity, req.ProductionRate,
                req.IsOrderBasedPlanningEnabled,
                req.ProductGroupCategoryIds ?? [],
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        var code = result.Value!;
        return CreatedAtAction(nameof(GetByCode), new { code }, new ProductionTeamCreatedResult(code));
    }

    [HttpPut("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateProductionTeamRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateProductionTeamCommand(
                code, req.Name, req.OrgUnitId,
                req.StandardLaborQuantity, req.ProductionRate,
                req.IsOrderBasedPlanningEnabled,
                req.ProductGroupCategoryIds ?? [],
                req.IsActive, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string code, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new DeleteProductionTeamCommand(code, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }

    [HttpPost("{code}/duplicate")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<ProductionTeamCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Duplicate(string code, [FromBody] DuplicateTeamRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DuplicateProductionTeamCommand(code, req.NewCode, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        var newCode = result.Value!;
        return CreatedAtAction(nameof(GetByCode), new { code = newCode }, new ProductionTeamCreatedResult(newCode));
    }

    // ── Worker roster sub-resource ──────────────────────────────────────────

    [HttpPost("{code}/members")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<TeamMemberAddedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddMember(string code, [FromBody] AddTeamMemberRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AddTeamMemberCommand(code, req.EmployeeCode, req.IsLeader, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetByCode), new { code }, new TeamMemberAddedResult(result.Value!));
    }

    [HttpDelete("{code}/members/{employeeCode}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(string code, string employeeCode, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new RemoveTeamMemberCommand(code, employeeCode, User.Identity?.Name), null, ct);
        return NoContent();
    }
}

public record CreateProductionTeamRequest(
    string Code,
    string Name,
    int? OrgUnitId,
    int? StandardLaborQuantity,
    decimal? ProductionRate,
    bool IsOrderBasedPlanningEnabled,
    IReadOnlyList<int>? ProductGroupCategoryIds);

public record UpdateProductionTeamRequest(
    string Name,
    int? OrgUnitId,
    int? StandardLaborQuantity,
    decimal? ProductionRate,
    bool IsOrderBasedPlanningEnabled,
    IReadOnlyList<int>? ProductGroupCategoryIds,
    bool IsActive);

public record DuplicateTeamRequest(string NewCode);
public record AddTeamMemberRequest(string EmployeeCode, bool IsLeader = false);
public record ProductionTeamCreatedResult(string TeamCode);
public record TeamMemberAddedResult(int MemberId);
