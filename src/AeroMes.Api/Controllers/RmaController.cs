using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.AddRmaLine;
using AeroMes.Application.Wms.Commands.AuthorizeRma;
using AeroMes.Application.Wms.Commands.CancelRma;
using AeroMes.Application.Wms.Commands.CloseRma;
using AeroMes.Application.Wms.Commands.CreateRma;
using AeroMes.Application.Wms.Commands.DisposeRmaLine;
using AeroMes.Application.Wms.Commands.ReceiveRma;
using AeroMes.Application.Wms.Queries.GetRmaById;
using AeroMes.Application.Wms.Queries.GetRmaList;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/rma")]
[Authorize]
public class RmaController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.RmaRead)]
    [ProducesResponseType<IReadOnlyList<RmaSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] ReturnDirection? direction,
        [FromQuery] RmaStatus? status,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetRmaListQuery(direction, status), null, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.RmaRead)]
    [ProducesResponseType<RmaDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetRmaByIdQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.RmaCreate)]
    [ProducesResponseType<RmaCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRmaRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new CreateRmaCommand(
            request.ReturnDirection,
            request.SourceDocumentType,
            request.SourceDocumentId,
            request.ReturnReason,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.RmaId }, result.Value);
    }

    [HttpPost("{id:int}/lines")]
    [RequirePermission(Permissions.RmaCreate)]
    [ProducesResponseType<RmaLineAddedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddLine(
        int id, [FromBody] AddRmaLineRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new AddRmaLineCommand(
            id,
            request.ProductCode,
            request.LotNumber,
            request.ReturnQty,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPost("{id:int}/authorize")]
    [RequirePermission(Permissions.RmaAuthorize)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Authorize(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AuthorizeRmaCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/receive")]
    [RequirePermission(Permissions.RmaReceive)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Receive(
        int id, [FromBody] ReceiveRmaRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new ReceiveRmaCommand(
            id,
            request.LineReceipts,
            request.QuarantineBinId,
            request.QuarantineLocationId,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/lines/{lineId:int}/dispose")]
    [RequirePermission(Permissions.RmaDispose)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DisposeLine(
        int id, int lineId, [FromBody] DisposeRmaLineRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new DisposeRmaLineCommand(
            id,
            lineId,
            request.Disposition,
            request.DispositionLocationId,
            request.DispositionBinId,
            request.Notes,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/close")]
    [RequirePermission(Permissions.RmaAuthorize)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Close(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CloseRmaCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.RmaCreate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CancelRmaCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateRmaRequest(
    ReturnDirection ReturnDirection,
    string? SourceDocumentType,
    int? SourceDocumentId,
    string ReturnReason);

public record AddRmaLineRequest(
    string ProductCode,
    string LotNumber,
    decimal ReturnQty);

public record ReceiveRmaRequest(
    IReadOnlyList<RmaLineReceiptEntry> LineReceipts,
    int QuarantineLocationId,
    int? QuarantineBinId);

public record DisposeRmaLineRequest(
    RmaDisposition Disposition,
    int DispositionLocationId,
    int? DispositionBinId,
    string? Notes);
