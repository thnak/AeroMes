using AeroMes.Application.Master.StorageLocations.Commands.CreateStorageLocation;
using AeroMes.Application.Master.StorageLocations.Commands.DeleteStorageLocation;
using AeroMes.Application.Master.StorageLocations.Commands.UpdateStorageLocation;
using AeroMes.Application.Master.StorageLocations.Queries.GetStorageLocations;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/storage-locations")]
[Authorize]
public class StorageLocationsController(ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<StorageLocationDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetStorageLocationsQuery(activeOnly), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<StorageLocationCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateStorageLocationRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateStorageLocationCommand(req.Code, req.Name, req.LocationType, req.WorkCenterId), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetAll), null, new StorageLocationCreatedResult(result.Value!));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStorageLocationRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new UpdateStorageLocationCommand(id, req.Name, req.LocationType, req.WorkCenterId), null, ct);
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
        var result = await commandMediator.SendAsync(new DeleteStorageLocationCommand(id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateStorageLocationRequest(string Code, string Name, LocationType LocationType, int? WorkCenterId = null);
public record UpdateStorageLocationRequest(string Name, LocationType LocationType, int? WorkCenterId = null);
public record StorageLocationCreatedResult(int StorageLocationId);
