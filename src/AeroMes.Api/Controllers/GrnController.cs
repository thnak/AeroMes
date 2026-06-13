using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.AddGrnLine;
using AeroMes.Application.Wms.Commands.ConfirmGrn;
using AeroMes.Application.Wms.Commands.CreateGrn;
using AeroMes.Application.Wms.Queries.GetGrnDetail;
using AeroMes.Application.Wms.Queries.GetGrnList;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/warehouse/grn")]
[Authorize]
public class GrnController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.WarehouseRead)]
    [ProducesResponseType<ApiResponse<IReadOnlyList<GrnListDto>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GrnListDto>>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] int? poId,
        CancellationToken ct)
    {
        GrnStatus? grnStatus = null;
        if (status is not null && Enum.TryParse<GrnStatus>(status, true, out var parsed))
            grnStatus = parsed;

        var result = await queryMediator.QueryAsync(new GetGrnListQuery(grnStatus, poId), null, ct);
        return Ok(new ApiResponse<IReadOnlyList<GrnListDto>>(true, "OK", result));
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.WarehouseRead)]
    [ProducesResponseType<ApiResponse<GrnDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<GrnDetailDto>>> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetGrnDetailQuery(id), null, ct);
        return Ok(new ApiResponse<GrnDetailDto>(true, "OK", result));
    }

    [HttpPost]
    [RequirePermission(Permissions.WarehouseReceive)]
    [ProducesResponseType<ApiResponse<GrnCreatedResult>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ApiResponse<GrnCreatedResult>>> Create(
        [FromBody] CreateGrnRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new CreateGrnCommand(
            request.GrnCode,
            request.PoId,
            request.StorageLocationId,
            request.ReceivedBy,
            request.ReceivedAt,
            request.DeliveryNoteRef,
            request.Notes,
            User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created,
            new ApiResponse<GrnCreatedResult>(true, "GRN created.", result.Value!));
    }

    [HttpPost("{grnId:int}/lines")]
    [RequirePermission(Permissions.WarehouseReceive)]
    [ProducesResponseType<ApiResponse<GrnLineAddedResult>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ApiResponse<GrnLineAddedResult>>> AddLine(
        int grnId,
        [FromBody] AddGrnLineRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new AddGrnLineCommand(
            grnId,
            request.PoLineId,
            request.ProductCode,
            request.LotNumber,
            request.ReceivedQty,
            request.ManufacturedDate,
            request.ExpiryDate,
            request.GrossWeightKg,
            request.DestinationBinId,
            User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created,
            new ApiResponse<GrnLineAddedResult>(true, "GRN line added.", result.Value!));
    }

    [HttpPost("{grnId:int}/confirm")]
    [RequirePermission(Permissions.WarehouseReceive)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Confirm(int grnId, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ConfirmGrnCommand(grnId, User.Identity?.Name ?? string.Empty), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateGrnRequest(
    string GrnCode,
    int? PoId,
    int StorageLocationId,
    string ReceivedBy,
    DateTime ReceivedAt,
    string? DeliveryNoteRef,
    string? Notes);

public record AddGrnLineRequest(
    int? PoLineId,
    string ProductCode,
    string LotNumber,
    decimal ReceivedQty,
    DateOnly? ManufacturedDate,
    DateOnly? ExpiryDate,
    decimal? GrossWeightKg,
    int? DestinationBinId);
