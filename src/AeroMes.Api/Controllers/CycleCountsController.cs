using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.ApproveCycleCount;
using AeroMes.Application.Wms.Commands.CreateCycleCountPlan;
using AeroMes.Application.Wms.Commands.DeleteCycleCountPlan;
using AeroMes.Application.Wms.Commands.GenerateCycleCountLines;
using AeroMes.Application.Wms.Commands.RecordCycleCountLine;
using AeroMes.Application.Wms.Commands.SubmitCycleCountForApproval;
using AeroMes.Application.Wms.Queries.GetCycleCountPlanById;
using AeroMes.Application.Wms.Queries.GetCycleCountPlans;
using AeroMes.Application.Wms.Queries.GetCycleCountSheet;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/cycle-counts")]
[Authorize]
public class CycleCountsController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.CycleCountRead)]
    [ProducesResponseType<IReadOnlyList<CycleCountPlanSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] CycleCountPlanStatus? status,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetCycleCountPlansQuery(status), null, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.CycleCountRead)]
    [ProducesResponseType<CycleCountPlanDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetCycleCountPlanByIdQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id:int}/sheet")]
    [RequirePermission(Permissions.CycleCountRead)]
    [ProducesResponseType<IReadOnlyList<CycleCountSheetLineDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSheet(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetCycleCountSheetQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.CycleCountCreate)]
    [ProducesResponseType<CycleCountPlanCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCycleCountPlanRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new CreateCycleCountPlanCommand(
            request.PlanType,
            request.ScheduledDate,
            request.Notes,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.PlanId }, result.Value);
    }

    [HttpPost("{id:int}/generate-lines")]
    [RequirePermission(Permissions.CycleCountCreate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GenerateLines(
        int id,
        [FromBody] GenerateCycleCountLinesRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new GenerateCycleCountLinesCommand(
            id,
            request.BinIds,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/record-line")]
    [RequirePermission(Permissions.CycleCountExecute)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RecordLine(
        int id,
        [FromBody] RecordCycleCountLineRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new RecordCycleCountLineCommand(
            id,
            request.LineId,
            request.CountedQty,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/submit")]
    [RequirePermission(Permissions.CycleCountExecute)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Submit(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SubmitCycleCountForApprovalCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/approve")]
    [RequirePermission(Permissions.CycleCountApprove)]
    [ProducesResponseType<ApproveCycleCountResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Approve(
        int id,
        [FromBody] ApproveCycleCountRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new ApproveCycleCountCommand(
            id,
            User.Identity?.Name,
            request.VarianceThresholdPct,
            request.Notes), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value!);
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.CycleCountDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteCycleCountPlanCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateCycleCountPlanRequest(
    CycleCountPlanType PlanType,
    DateOnly ScheduledDate,
    string? Notes);

public record GenerateCycleCountLinesRequest(int[]? BinIds);

public record RecordCycleCountLineRequest(long LineId, decimal CountedQty);

public record ApproveCycleCountRequest(decimal VarianceThresholdPct, string? Notes);
