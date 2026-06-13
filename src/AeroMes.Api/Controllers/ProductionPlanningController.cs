using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Production.Commands.CreateProductionPlan;
using AeroMes.Application.Production.Commands.UpdateProductionPlanLine;
using AeroMes.Application.Production.Commands.UpdateProductionPlanStatus;
using AeroMes.Application.Production.Queries.GetProductionPlanGantt;
using AeroMes.Application.Production.Queries.GetProductionPlans;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/production-planning")]
[Authorize]
public class ProductionPlanningController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpPost]
    [RequirePermission(Permissions.ProductionPlanningWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePlan(
        [FromBody] CreateProductionPlanRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateProductionPlanCommand(
                request.PoId, request.AllocationMethod, request.Lines, request.Notes),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpGet]
    [RequirePermission(Permissions.ProductionPlanningRead)]
    [ProducesResponseType<PagedResult<ProductionPlanDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] int? poId,
        [FromQuery] ProductionPlanStatus? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetProductionPlansQuery(poId, status, from, to, page, pageSize), null, ct));

    [HttpPost("{id:int}/status")]
    [RequirePermission(Permissions.ProductionPlanningWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        int id, [FromBody] UpdatePlanStatusRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateProductionPlanStatusCommand(id, request.NewStatus), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPut("{id:int}/lines/{lineId:int}")]
    [RequirePermission(Permissions.ProductionPlanningWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLine(
        int id, int lineId,
        [FromBody] UpdatePlanLineRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateProductionPlanLineCommand(id, lineId, request.TeamCode,
                request.PlannedStartDate, request.PlannedEndDate), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpGet("gantt")]
    [RequirePermission(Permissions.ProductionPlanningRead)]
    [ProducesResponseType<IReadOnlyList<GanttTeamDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGantt(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string? teamCode = null,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetProductionPlanGanttQuery(from, to, teamCode), null, ct));
}

public record CreateProductionPlanRequest(
    int PoId,
    PlanAllocationMethod AllocationMethod,
    IReadOnlyList<PlanLineInput> Lines,
    string? Notes = null);

public record UpdatePlanStatusRequest(ProductionPlanStatus NewStatus);

public record UpdatePlanLineRequest(
    string? TeamCode,
    DateTime? PlannedStartDate,
    DateTime? PlannedEndDate);
