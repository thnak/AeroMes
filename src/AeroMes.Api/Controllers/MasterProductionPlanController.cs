using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Production.MasterPlan.Commands.ApproveMasterPlan;
using AeroMes.Application.Production.MasterPlan.Commands.CreateMasterPlan;
using AeroMes.Application.Production.MasterPlan.Commands.DeleteMasterPlan;
using AeroMes.Application.Production.MasterPlan.Commands.DistributeMasterPlan;
using AeroMes.Application.Production.MasterPlan.Commands.UpdateMasterPlan;
using AeroMes.Application.Production.MasterPlan.Queries.GetMasterPlanDetail;
using AeroMes.Application.Production.MasterPlan.Queries.GetMasterPlans;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/production-plans/master")]
[Authorize]
public sealed class MasterProductionPlanController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MpsRead)]
    [ProducesResponseType<IReadOnlyList<MasterPlanSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? orgUnit,
        [FromQuery] string? status,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(
            new GetMasterPlansQuery(orgUnit, status, from, to), null, ct));

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.MpsRead)]
    [ProducesResponseType<MasterPlanDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetMasterPlanDetailQuery(id), null, ct);
        if (!result.IsFound) return NotFound(result.ErrorMessage);
        return Ok(result.Value);
    }

    [HttpPost]
    [RequirePermission(Permissions.MpsWrite)]
    [ProducesResponseType<int>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMasterPlanRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateMasterPlanCommand(
                request.PlanNumber, request.PlanName, request.OrganizationalUnit,
                request.Granularity, request.PeriodStart, request.PeriodEnd,
                request.DataSource, request.WorkingHoursPerDay, request.WorkingDaysPerWeek,
                request.Lines, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetDetail), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MpsWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateMasterPlanRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateMasterPlanCommand(
                id, request.PlanName, request.OrganizationalUnit,
                request.WorkingHoursPerDay, request.WorkingDaysPerWeek,
                request.Lines, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MpsWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteMasterPlanCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/distribute")]
    [RequirePermission(Permissions.MpsWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Distribute(
        int id, [FromBody] DistributeMasterPlanRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DistributeMasterPlanCommand(id, request.Strategy, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/approve")]
    [RequirePermission(Permissions.MpsWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ApproveMasterPlanCommand(id, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateMasterPlanRequest(
    string? PlanNumber,
    string PlanName,
    string? OrganizationalUnit,
    MpsGranularity Granularity,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    MpsDataSource DataSource,
    decimal WorkingHoursPerDay,
    int WorkingDaysPerWeek,
    IReadOnlyList<MasterPlanLineInput> Lines);

public record UpdateMasterPlanRequest(
    string PlanName,
    string? OrganizationalUnit,
    decimal WorkingHoursPerDay,
    int WorkingDaysPerWeek,
    IReadOnlyList<MasterPlanLineInput> Lines);

public record DistributeMasterPlanRequest(MpsDistributionStrategy Strategy);
