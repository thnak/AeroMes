using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Traceability.Commands.BulkHoldFromForwardTrace;
using AeroMes.Application.Traceability.Commands.PlaceHold;
using AeroMes.Application.Traceability.Commands.RejectDisposition;
using AeroMes.Application.Traceability.Commands.ReleaseHold;
using AeroMes.Application.Traceability.Queries.CheckLotHoldStatus;
using AeroMes.Application.Traceability.Queries.GetActiveHolds;
using AeroMes.Application.Traceability.Queries.GetHoldHistory;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/trace/holds")]
[Authorize]
public class TraceHoldsController(
    IQueryMediator queryMediator,
    ICommandMediator commandMediator) : ControllerBase
{
    [HttpPost]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PlaceHold(
        [FromBody] PlaceHoldRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new PlaceHoldCommand(
                req.LotNumber, req.HoldReason, User.Identity?.Name ?? "system",
                req.ProductCode, req.WorkOrderID, req.HoldDescription, req.HoldReference), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created, result.Value!);
    }

    [HttpPost("{id:guid}/release")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ReleaseHold(
        Guid id, [FromBody] ReleaseHoldRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ReleaseHoldCommand(
                id, req.DispositionCode, req.DispositionNotes,
                User.Identity?.Name ?? "system", req.ESignatureToken), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RejectDisposition(
        Guid id, [FromBody] RejectDispositionRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RejectDispositionCommand(
                id, req.DispositionCode, req.DispositionNotes,
                User.Identity?.Name ?? "system", req.ESignatureToken), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("bulk-from-forward-trace")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType<BulkHoldResultDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> BulkHoldFromForwardTrace(
        [FromBody] BulkHoldFromForwardTraceRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new BulkHoldFromForwardTraceCommand(
                req.SuspectLotNumber, req.HoldReason, req.HoldReference,
                User.Identity?.Name ?? "system", req.HoldDescription, req.MaxDepth), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created, result.Value!);
    }

    [HttpGet]
    [ProducesResponseType<PagedResult<LotHoldDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveHolds(
        [FromQuery] string? lotNumber,
        [FromQuery] string? reason,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await queryMediator.QueryAsync(
            new GetActiveHoldsQuery(lotNumber, reason, page, pageSize), null, ct);
        return Ok(result);
    }

    [HttpGet("lot/{lotNumber}/history")]
    [ProducesResponseType<IReadOnlyList<LotHoldDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHoldHistory(string lotNumber, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetHoldHistoryQuery(lotNumber), null, ct);
        return Ok(result);
    }

    [HttpGet("lot/{lotNumber}/status")]
    [ProducesResponseType<LotHoldStatusDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckHoldStatus(string lotNumber, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new CheckLotHoldStatusQuery(lotNumber), null, ct);
        return Ok(result);
    }
}

public record PlaceHoldRequest(
    string LotNumber,
    HoldReason HoldReason,
    string? ProductCode,
    int? WorkOrderID,
    string? HoldDescription,
    string? HoldReference);

public record ReleaseHoldRequest(
    HoldDispositionCode DispositionCode,
    string? DispositionNotes,
    string ESignatureToken);

public record RejectDispositionRequest(
    HoldDispositionCode DispositionCode,
    string DispositionNotes,
    string ESignatureToken);

public record BulkHoldFromForwardTraceRequest(
    string SuspectLotNumber,
    HoldReason HoldReason,
    string HoldReference,
    string? HoldDescription,
    int MaxDepth = 20);
