using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.ConfirmPurchaseOrder;
using AeroMes.Application.Wms.Commands.CreatePurchaseOrder;
using AeroMes.Application.Wms.Queries.GetPurchaseOrders;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/warehouse/purchase-orders")]
[Authorize]
public class PurchaseOrdersController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.WarehouseRead)]
    [ProducesResponseType<ApiResponse<IReadOnlyList<PurchaseOrderDto>>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PurchaseOrderDto>>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? supplierCode,
        CancellationToken ct)
    {
        PoStatus? poStatus = null;
        if (status is not null && Enum.TryParse<PoStatus>(status, true, out var parsed))
            poStatus = parsed;

        var result = await queryMediator.QueryAsync(new GetPurchaseOrdersQuery(poStatus, supplierCode), null, ct);
        return Ok(new ApiResponse<IReadOnlyList<PurchaseOrderDto>>(true, "OK", result));
    }

    [HttpPost]
    [RequirePermission(Permissions.WarehouseReceive)]
    [ProducesResponseType<ApiResponse<PoCreatedResult>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ApiResponse<PoCreatedResult>>> Create(
        [FromBody] CreatePurchaseOrderRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new CreatePurchaseOrderCommand(
            request.PoCode,
            request.SupplierCode,
            request.ExpectedDeliveryDate,
            request.Lines,
            request.Notes,
            User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created,
            new ApiResponse<PoCreatedResult>(true, "Purchase order created.", result.Value!));
    }

    [HttpPost("{id:int}/confirm")]
    [RequirePermission(Permissions.WarehouseReceive)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Confirm(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ConfirmPurchaseOrderCommand(id, User.Identity?.Name ?? string.Empty), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreatePurchaseOrderRequest(
    string PoCode,
    string SupplierCode,
    DateOnly ExpectedDeliveryDate,
    IReadOnlyList<CreatePoLineRequest> Lines,
    string? Notes);
