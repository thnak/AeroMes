using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.CreateBeginningInventoryEntry;
using AeroMes.Application.Wms.Commands.DeleteBeginningInventoryEntry;
using AeroMes.Application.Wms.Commands.UpdateBeginningInventoryEntry;
using AeroMes.Application.Wms.Queries.GetBeginningInventoryEntries;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/factory-beginning-inventory")]
[Authorize]
public class FactoryInventoryController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.InventoryRead)]
    [ProducesResponseType<IReadOnlyList<BeginningInventoryEntryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? warehouseId,
        [FromQuery] string? productCode,
        [FromQuery] DateOnly? period,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetBeginningInventoryEntriesQuery(warehouseId, productCode, period), null, ct);
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.BeginningInventoryWrite)]
    [ProducesResponseType<BeginningInventoryEntryCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBeginningInventoryEntryRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new CreateBeginningInventoryEntryCommand(
            request.Period,
            request.WarehouseId,
            request.ProductCode,
            request.UnitOfMeasure,
            request.BeginningQuantity,
            request.LotNumber,
            request.ExpirationDate,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetAll), result.Value!);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.BeginningInventoryWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateBeginningInventoryEntryRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new UpdateBeginningInventoryEntryCommand(
            id,
            request.BeginningQuantity,
            request.LotNumber,
            request.ExpirationDate,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.BeginningInventoryWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteBeginningInventoryEntryCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateBeginningInventoryEntryRequest(
    DateOnly Period,
    int WarehouseId,
    string ProductCode,
    string UnitOfMeasure,
    decimal BeginningQuantity,
    string? LotNumber,
    DateOnly? ExpirationDate);

public record UpdateBeginningInventoryEntryRequest(
    decimal BeginningQuantity,
    string? LotNumber,
    DateOnly? ExpirationDate);
