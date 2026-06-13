using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Production.DetailedPlan.Commands.CalculateDetailedPlan;
using AeroMes.Application.Production.DetailedPlan.Commands.CreateDetailedPlan;
using AeroMes.Application.Production.DetailedPlan.Commands.DeleteDetailedPlan;
using AeroMes.Application.Production.DetailedPlan.Commands.FinalizeDetailedPlan;
using AeroMes.Application.Production.DetailedPlan.Commands.UpdateDetailedPlan;
using AeroMes.Application.Production.DetailedPlan.Queries.GetDetailedPlanDetail;
using AeroMes.Application.Production.DetailedPlan.Queries.GetDetailedPlans;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/production-plans/detailed")]
[Authorize]
public sealed class DetailedProductionPlanController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MpsRead)]
    [ProducesResponseType<IReadOnlyList<DetailedPlanSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] int? masterPlanId,
        [FromQuery] string? orgUnit,
        [FromQuery] string? status,
        CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(
            new GetDetailedPlansQuery(masterPlanId, orgUnit, status), null, ct));

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.MpsRead)]
    [ProducesResponseType<DetailedPlanDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetDetailedPlanDetailQuery(id), null, ct);
        if (!result.IsFound) return NotFound(result.ErrorMessage);
        return Ok(result.Value);
    }

    [HttpPost]
    [RequirePermission(Permissions.MpsWrite)]
    [ProducesResponseType<int>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateDetailedPlanRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateDetailedPlanCommand(
                request.MasterPlanId, request.PlanNumber, request.PlanName,
                request.Granularity, request.ProductLines, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetDetail), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MpsWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateDetailedPlanRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateDetailedPlanCommand(
                id, request.PlanName, request.Granularity,
                request.ProductLines, User.Identity?.Name), null, ct);
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
            new DeleteDetailedPlanCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/calculate")]
    [RequirePermission(Permissions.MpsWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Calculate(
        int id, [FromBody] CalculateDetailedPlanRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CalculateDetailedPlanCommand(
                id, request.Strategy, request.WorkingDaysPerWeek,
                request.ShiftLabels, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/finalize")]
    [RequirePermission(Permissions.MpsWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Finalize(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new FinalizeDetailedPlanCommand(id, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateDetailedPlanRequest(
    int MasterPlanId,
    string? PlanNumber,
    string PlanName,
    DppGranularity Granularity,
    IReadOnlyList<DppProductLineInput> ProductLines);

public record UpdateDetailedPlanRequest(
    string PlanName,
    DppGranularity Granularity,
    IReadOnlyList<DppProductLineInput> ProductLines);

public record CalculateDetailedPlanRequest(
    DppDistributionStrategy Strategy,
    int WorkingDaysPerWeek,
    IReadOnlyList<string>? ShiftLabels);
