using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Master.Warehouses.Commands.CreateWarehouse;
using AeroMes.Application.Master.Warehouses.Commands.DeactivateWarehouse;
using AeroMes.Application.Master.Warehouses.Commands.DeleteWarehouse;
using AeroMes.Application.Master.Warehouses.Commands.UpdateWarehouse;
using AeroMes.Application.Master.Warehouses.Queries.GetWarehouses;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/warehouses")]
[Authorize]
public class WarehousesController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<WarehouseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool activeOnly = true,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetWarehousesQuery(activeOnly, search), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<WarehouseCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateWarehouseCommand(
                req.Code,
                req.Name,
                req.WarehouseType,
                req.Address,
                req.IntegrationSource,
                User.Identity?.Name),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetAll), null, new WarehouseCreatedResult(result.Value!));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWarehouseRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateWarehouseCommand(id, req.Name, req.Address, req.WarehouseType, User.Identity?.Name ?? "system"),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("{id:int}/deactivate")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeactivateWarehouseCommand(id, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteWarehouseCommand(id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateWarehouseRequest(
    string Code,
    string Name,
    WarehouseType WarehouseType,
    string? Address,
    IntegrationSource IntegrationSource);

public record UpdateWarehouseRequest(
    string Name,
    string? Address,
    WarehouseType WarehouseType);

public record WarehouseCreatedResult(int WarehouseId);
