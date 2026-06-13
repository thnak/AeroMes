using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Integration.Commands.CreateMultiProductionOrder;
using AeroMes.Application.Integration.Queries.GetMultiProductionOrderDetail;
using AeroMes.Application.Integration.Queries.GetMultiProductionOrders;
using AeroMes.Domain.Integration;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/production-orders/multi-product")]
[Authorize]
public class MultiProductionOrdersController(
    IQueryMediator queryMediator,
    ICommandMediator commandMediator) : ControllerBase
{
    [HttpPost]
    [RequirePermission(Permissions.IntegrationSync)]
    [ProducesResponseType<MultiProductionOrderCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMultiProductionOrderRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateMultiProductionOrderCommand(
                req.OrderType,
                req.SourceReference,
                req.PlannedStart,
                req.PlannedEnd,
                req.Priority,
                req.ProductionUnit,
                req.Notes,
                [.. req.Lines.Select(l => new CreateMpoLineItem(
                    l.ProductCode, l.PlannedQty, l.UoMCode, l.BomVersion))],
                User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created, result.Value!);
    }

    [HttpGet]
    [RequirePermission(Permissions.IntegrationRead)]
    [ProducesResponseType<IReadOnlyList<MultiProductionOrderSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] MultiProductionOrderType? orderType,
        [FromQuery] MultiProductionOrderStatus? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetMultiProductionOrdersQuery(orderType, status, from, to), null, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.IntegrationRead)]
    [ProducesResponseType<MultiProductionOrderDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetMultiProductionOrderDetailQuery(id), null, ct);
        if (!result.IsFound) return NotFound(result.ErrorMessage);
        return Ok(result.Value!);
    }
}

public record CreateMpoLineRequest(
    string ProductCode,
    int PlannedQty,
    string UoMCode,
    string? BomVersion);

public record CreateMultiProductionOrderRequest(
    MultiProductionOrderType OrderType,
    string? SourceReference,
    DateTime? PlannedStart,
    DateTime? PlannedEnd,
    byte Priority,
    string? ProductionUnit,
    string? Notes,
    IReadOnlyList<CreateMpoLineRequest> Lines);
