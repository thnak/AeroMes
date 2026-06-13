using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Integration.Commands.CreateProductionOrder;
using AeroMes.Application.Integration.Commands.DeleteProductionOrder;
using AeroMes.Application.Integration.Commands.UpdateProductionOrderStatus;
using AeroMes.Application.Integration.Queries.GetProductionOrderDetail;
using AeroMes.Application.Integration.Queries.GetProductionOrders;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/production-orders")]
[Authorize]
public sealed class ProductionOrdersController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.ProductionPlanningRead)]
    [ProducesResponseType<IReadOnlyList<ProductionOrderDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] int? soId,
        [FromQuery] string? poCode,
        [FromQuery] string? productCode,
        [FromQuery] string? status,
        CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(
            new GetProductionOrdersQuery(soId, poCode, productCode, status), null, ct));

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.ProductionPlanningRead)]
    [ProducesResponseType<ProductionOrderDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetProductionOrderDetailQuery(id), null, ct);
        if (!result.IsFound) return NotFound(result.ErrorMessage);
        return Ok(result.Value);
    }

    [HttpPost]
    [RequirePermission(Permissions.ProductionPlanningWrite)]
    [ProducesResponseType<int>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductionOrderRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateProductionOrderCommand(
                request.ProductCode,
                request.TargetQuantity,
                request.PlannedStartDate,
                request.PlannedEndDate,
                request.Deadline,
                request.Priority,
                request.AssignedTo,
                request.SoId,
                request.RoutingId,
                request.AutoExpandBom,
                User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetDetail), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id:int}/status")]
    [RequirePermission(Permissions.ProductionPlanningWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        int id, [FromBody] UpdatePoStatusRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateProductionOrderStatusCommand(id, request.Action), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.ProductionPlanningWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteProductionOrderCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateProductionOrderRequest(
    string ProductCode,
    int TargetQuantity,
    DateTime? PlannedStartDate,
    DateTime? PlannedEndDate,
    DateTime? Deadline,
    byte Priority,
    string? AssignedTo,
    int? SoId,
    int? RoutingId,
    bool AutoExpandBom);

public record UpdatePoStatusRequest(string Action);
