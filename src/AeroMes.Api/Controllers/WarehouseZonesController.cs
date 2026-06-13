using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Wms.Commands.CreateZone;
using AeroMes.Application.Wms.Commands.DeleteZone;
using AeroMes.Application.Wms.Commands.UpdateZone;
using AeroMes.Application.Wms.Queries.GetWarehouseMap;
using AeroMes.Application.Wms.Queries.GetZones;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/warehouse/zones")]
[Authorize]
public class WarehouseZonesController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.WarehouseRead)]
    [ProducesResponseType<IReadOnlyList<ZoneDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? storageLocationId = null,
        [FromQuery] bool activeOnly = true,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetZonesQuery(storageLocationId, activeOnly), null, ct));

    [HttpGet("map")]
    [RequirePermission(Permissions.WarehouseRead)]
    [ProducesResponseType<ZoneMapDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMap([FromQuery] int zoneId, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetWarehouseMapQuery(zoneId), null, ct);
        if (!result.Any()) return NotFound();
        return Ok(result[0]);
    }

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<ZoneCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateZoneRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateZoneCommand(
                req.ZoneCode, req.ZoneName, req.ZoneType,
                req.StorageLocationId, req.WarehouseId, req.TemperatureZone,
                User.Identity?.Name),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetAll), null, new ZoneCreatedResult(result.Value!));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateZoneRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateZoneCommand(id, req.ZoneName, req.ZoneType, req.TemperatureZone, User.Identity?.Name ?? "system"),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteZoneCommand(id, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateZoneRequest(
    string ZoneCode,
    string ZoneName,
    ZoneType ZoneType,
    int StorageLocationId,
    int? WarehouseId,
    TemperatureZone? TemperatureZone);

public record UpdateZoneRequest(string ZoneName, ZoneType ZoneType, TemperatureZone? TemperatureZone);
public record ZoneCreatedResult(int ZoneId);
