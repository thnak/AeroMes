using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Production.Commands.CompletePackagingOrder;
using AeroMes.Application.Production.Commands.CreatePackagingBom;
using AeroMes.Application.Production.Commands.CreatePackagingOrder;
using AeroMes.Application.Production.Commands.PrintPackagingLabel;
using AeroMes.Application.Production.Commands.RecordPackagedQty;
using AeroMes.Application.Production.Queries.GetPackagingBoms;
using AeroMes.Application.Production.Queries.GetPackagingOrders;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/packaging")]
[Authorize]
public class PackagingController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpPost("boms")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBom([FromBody] CreatePackagingBomRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreatePackagingBomCommand(
                request.ProductCode,
                request.Lines.Select(l => new PackagingBomLineInput(l.MaterialCode, l.Quantity, l.UnitCode, l.Notes)).ToList(),
                request.Notes),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpGet("boms")]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<IReadOnlyList<PackagingBomDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBoms([FromQuery] string? productCode, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetPackagingBomsQuery(productCode), null, ct));

    [HttpPost("orders")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateOrder([FromBody] CreatePackagingOrderRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreatePackagingOrderCommand(request.WOID, request.ProductCode, request.PlannedQty, request.Notes),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpGet("orders")]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<IReadOnlyList<PackagingOrderDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int? woid,
        [FromQuery] PackagingOrderStatus? status,
        CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetPackagingOrdersQuery(woid, status), null, ct));

    [HttpPost("orders/{id:int}/record")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordPackaged(int id, [FromBody] RecordPackagedRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new RecordPackagedQtyCommand(id, request.Qty), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPost("orders/{id:int}/complete")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteOrder(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new CompletePackagingOrderCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPost("orders/{id:int}/labels")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PrintLabel(int id, [FromBody] PrintLabelRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new PrintPackagingLabelCommand(id, request.LabelTemplate), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }
}

public record PackagingBomLineRequest(string MaterialCode, decimal Quantity, string UnitCode, string? Notes = null);
public record CreatePackagingBomRequest(string ProductCode, IReadOnlyList<PackagingBomLineRequest> Lines, string? Notes = null);
public record CreatePackagingOrderRequest(int WOID, string ProductCode, decimal PlannedQty, string? Notes = null);
public record RecordPackagedRequest(decimal Qty);
public record PrintLabelRequest(string? LabelTemplate = null);
