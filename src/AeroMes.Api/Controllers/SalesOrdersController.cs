using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Integration.Commands.CompleteSalesOrder;
using AeroMes.Application.Integration.Commands.ConfirmSalesOrder;
using AeroMes.Application.Integration.Commands.CreateSalesOrder;
using AeroMes.Application.Integration.Commands.RejectSalesOrder;
using AeroMes.Application.Integration.Queries.GetSalesOrderDetailWithLines;
using AeroMes.Application.Integration.Queries.GetSalesOrdersList;
using AeroMes.Domain.Integration.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/sales-orders")]
[Authorize]
public sealed class SalesOrdersController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.IntegrationRead)]
    [ProducesResponseType<IReadOnlyList<SalesOrderSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? soCode,
        [FromQuery] string? status,
        [FromQuery] bool includeUnconfirmed = false,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetSalesOrdersListQuery(soCode, status, includeUnconfirmed, from, to), null, ct));

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.IntegrationRead)]
    [ProducesResponseType<SalesOrderWithLinesDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetSalesOrderDetailWithLinesQuery(id), null, ct);
        if (!result.IsFound) return NotFound(result.ErrorMessage);
        return Ok(result.Value);
    }

    [HttpPost]
    [RequirePermission(Permissions.IntegrationRead)]
    [ProducesResponseType<int>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSalesOrderRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateSalesOrderCommand(
                request.SoCode, request.CustomerCode, request.CustomerName,
                request.OrderDate, request.DeliveryDate, request.Notes,
                request.Lines, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetDetail), new { id = result.Value }, result.Value);
    }

    [HttpPost("{id:int}/confirm")]
    [RequirePermission(Permissions.IntegrationSync)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(
        int id, [FromBody] ConfirmSoRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ConfirmSalesOrderCommand(id, request.FacilityCode, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/reject")]
    [RequirePermission(Permissions.IntegrationSync)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RejectSalesOrderCommand(id, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/complete")]
    [RequirePermission(Permissions.IntegrationSync)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CompleteSalesOrderCommand(id, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateSalesOrderRequest(
    string? SoCode,
    string? CustomerCode,
    string? CustomerName,
    DateTime OrderDate,
    DateTime? DeliveryDate,
    string? Notes,
    IReadOnlyList<SalesOrderLineInput> Lines);

public record ConfirmSoRequest(string? FacilityCode);
