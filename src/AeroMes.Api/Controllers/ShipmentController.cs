using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.AddCartonContent;
using AeroMes.Application.Wms.Commands.AddShipmentLine;
using AeroMes.Application.Wms.Commands.CancelShipment;
using AeroMes.Application.Wms.Commands.CompletePickList;
using AeroMes.Application.Wms.Commands.ConfirmPickLine;
using AeroMes.Application.Wms.Commands.CreateCarton;
using AeroMes.Application.Wms.Commands.CreatePickList;
using AeroMes.Application.Wms.Commands.CreateShipmentOrder;
using AeroMes.Application.Wms.Commands.DispatchShipment;
using AeroMes.Application.Wms.Commands.SealCarton;
using AeroMes.Application.Wms.Queries.GetPickListForShipment;
using AeroMes.Application.Wms.Queries.GetShipmentById;
using AeroMes.Application.Wms.Queries.GetShipmentList;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/shipments")]
[Authorize]
public class ShipmentController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.ShipmentRead)]
    [ProducesResponseType<IReadOnlyList<ShipmentSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] ShipmentStatus? status, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetShipmentListQuery(status), null, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.ShipmentRead)]
    [ProducesResponseType<ShipmentDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetShipmentByIdQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id:int}/pick-list")]
    [RequirePermission(Permissions.ShipmentPick)]
    [ProducesResponseType<PickListDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPickList(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetPickListForShipmentQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.ShipmentCreate)]
    [ProducesResponseType<ShipmentCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateShipmentRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new CreateShipmentOrderCommand(
            request.SoId, request.CustomerName, request.RequestedShipDate, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.ShipmentId }, result.Value!);
    }

    [HttpPost("{id:int}/lines")]
    [RequirePermission(Permissions.ShipmentCreate)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddLine(int id, [FromBody] AddShipmentLineRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AddShipmentLineCommand(id, request.ProductCode, request.OrderedQty, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created, result.Value!);
    }

    [HttpPost("{id:int}/pick-list")]
    [RequirePermission(Permissions.ShipmentPick)]
    [ProducesResponseType<PickListCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreatePickList(int id, [FromBody] CreatePickListRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new CreatePickListCommand(
            id, request.LocationId, request.AssignedTo, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created, result.Value!);
    }

    [HttpPost("pick-lines/{lineId:long}/confirm")]
    [RequirePermission(Permissions.ShipmentPick)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ConfirmPickLine(long lineId, [FromBody] ConfirmPickLineRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new ConfirmPickLineCommand(
            lineId, request.PickListId, request.ActualPickedQty,
            request.ScannedBinId, request.ScannedLotNumber, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("pick-lists/{pickListId:int}/complete")]
    [RequirePermission(Permissions.ShipmentPick)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CompletePickList(int pickListId, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CompletePickListCommand(pickListId, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/cartons")]
    [RequirePermission(Permissions.ShipmentPack)]
    [ProducesResponseType<CartonCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateCarton(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateCartonCommand(id, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created, result.Value!);
    }

    [HttpPost("cartons/{cartonId:int}/contents")]
    [RequirePermission(Permissions.ShipmentPack)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddCartonContent(int cartonId, [FromBody] AddCartonContentRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new AddCartonContentCommand(
            cartonId, request.ProductCode, request.LotNumber, request.PackedQty, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("cartons/{cartonId:int}/seal")]
    [RequirePermission(Permissions.ShipmentPack)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SealCarton(int cartonId, [FromBody] SealCartonRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SealCartonCommand(cartonId, request.GrossWeightKg, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/dispatch")]
    [RequirePermission(Permissions.ShipmentDispatch)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Dispatch(int id, [FromBody] DispatchShipmentRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new DispatchShipmentCommand(
            id, request.CarrierName, request.TrackingNumber, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.ShipmentCreate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CancelShipmentCommand(id, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateShipmentRequest(int? SoId, string CustomerName, DateOnly RequestedShipDate);
public record AddShipmentLineRequest(string ProductCode, decimal OrderedQty);
public record CreatePickListRequest(int LocationId, string? AssignedTo);
public record ConfirmPickLineRequest(int PickListId, decimal ActualPickedQty, int? ScannedBinId, string? ScannedLotNumber);
public record AddCartonContentRequest(string ProductCode, string LotNumber, decimal PackedQty);
public record SealCartonRequest(decimal? GrossWeightKg);
public record DispatchShipmentRequest(string? CarrierName, string? TrackingNumber);
