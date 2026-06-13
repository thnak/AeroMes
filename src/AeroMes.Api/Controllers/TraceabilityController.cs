using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Traceability.Commands.AppendLotEvent;
using AeroMes.Application.Traceability.Commands.RecordLotLineage;
using AeroMes.Application.Traceability.Queries.BackwardTrace;
using AeroMes.Application.Traceability.Queries.BidirectionalTrace;
using AeroMes.Application.Traceability.Queries.ForwardTrace;
using AeroMes.Application.Traceability.Queries.LotEventTimeline;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/traceability")]
[Authorize]
public class TraceabilityController(
    IQueryMediator queryMediator,
    ICommandMediator commandMediator) : ControllerBase
{
    [HttpGet("backward")]
    [ProducesResponseType<LotGenealogyDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> BackwardTrace(
        [FromQuery] string lotNumber, [FromQuery] int maxDepth = 20, CancellationToken ct = default)
    {
        var result = await queryMediator.QueryAsync(
            new BackwardTraceQuery(lotNumber, maxDepth), null, ct);
        return Ok(result);
    }

    [HttpGet("forward")]
    [ProducesResponseType<LotGenealogyDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForwardTrace(
        [FromQuery] string lotNumber, [FromQuery] int maxDepth = 20, CancellationToken ct = default)
    {
        var result = await queryMediator.QueryAsync(
            new ForwardTraceQuery(lotNumber, maxDepth), null, ct);
        return Ok(result);
    }

    [HttpGet("bidirectional")]
    [ProducesResponseType<LotGenealogyDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> BidirectionalTrace(
        [FromQuery] string lotNumber, [FromQuery] int maxDepth = 20, CancellationToken ct = default)
    {
        var result = await queryMediator.QueryAsync(
            new BidirectionalTraceQuery(lotNumber, maxDepth), null, ct);
        return Ok(result);
    }

    [HttpGet("lot-events/{lotNumber}")]
    [ProducesResponseType<IReadOnlyList<LotEventDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLotEventTimeline(
        string lotNumber,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new LotEventTimelineQuery(lotNumber, from, to), null, ct);
        return Ok(result);
    }

    [HttpPost("lineage")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RecordLineage(
        [FromBody] RecordLineageRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RecordLotLineageCommand(
                req.ParentLotNumber, req.ChildLotNumber, req.LineageType,
                req.WorkOrderID, req.RoutingStepID, req.QuantityConsumed, req.UoM), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("lot-events")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AppendEvent(
        [FromBody] AppendLotEventRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AppendLotEventCommand(
                req.EventType, req.LotNumber, req.ProductCode, req.OperatorCode,
                req.EventTimestamp, req.WorkOrderID, req.RoutingStepID, req.LocationID,
                req.Quantity, req.UoM, req.Payload, req.EquipmentCode, req.SourceSystem), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record RecordLineageRequest(
    string ParentLotNumber,
    string ChildLotNumber,
    LineageType LineageType,
    int? WorkOrderID,
    int? RoutingStepID,
    decimal? QuantityConsumed,
    string? UoM);

public record AppendLotEventRequest(
    LotEventType EventType,
    string LotNumber,
    string ProductCode,
    string OperatorCode,
    DateTime EventTimestamp,
    int? WorkOrderID,
    int? RoutingStepID,
    int? LocationID,
    decimal? Quantity,
    string? UoM,
    string? Payload,
    string? EquipmentCode,
    LotEventSourceSystem SourceSystem);
