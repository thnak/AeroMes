using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Production.Commands.CreateDisassemblyOrder;
using AeroMes.Application.Production.Commands.RecordDisassemblyRecovery;
using AeroMes.Application.Production.Commands.UpdateDisassemblyOrderStatus;
using AeroMes.Application.Production.Queries.GetDisassemblyOrders;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/disassembly-orders")]
[Authorize]
public class DisassemblyOrdersController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpPost]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDisassemblyOrderRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateDisassemblyOrderCommand(
                request.OrderType, request.SourceProductCode, request.SourceQty,
                request.PurchaseOrderID, request.Deadline, request.Notes),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpGet]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<PagedResult<DisassemblyOrderDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? sourceProductCode,
        [FromQuery] DisassemblyOrderStatus? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetDisassemblyOrdersQuery(sourceProductCode, status, from, to, page, pageSize), null, ct));

    [HttpPost("{id:int}/status")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        int id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateDisassemblyOrderStatusCommand(id, request.NewStatus), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPost("{id:int}/record")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordRecovery(
        int id, [FromBody] RecordRecoveryRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RecordDisassemblyRecoveryCommand(id, request.ProductCode, request.ActualQty), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }
}

public record CreateDisassemblyOrderRequest(
    DisassemblyOrderType OrderType,
    string SourceProductCode,
    decimal SourceQty,
    int? PurchaseOrderID = null,
    DateTime? Deadline = null,
    string? Notes = null);

public record UpdateStatusRequest(DisassemblyOrderStatus NewStatus);
public record RecordRecoveryRequest(string ProductCode, decimal ActualQty);
