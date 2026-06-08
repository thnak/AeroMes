using AeroMes.Application.Master.StorageLocations.Commands.CreateStorageLocation;
using AeroMes.Application.Master.StorageLocations.Commands.DeleteStorageLocation;
using AeroMes.Application.Master.StorageLocations.Commands.UpdateStorageLocation;
using AeroMes.Application.Master.StorageLocations.Queries.GetStorageLocations;
using AeroMes.Domain.Master;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/storage-locations")]
[Authorize]
public class StorageLocationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await mediator.Send(new GetStorageLocationsQuery(activeOnly), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStorageLocationRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(
            new CreateStorageLocationCommand(req.Code, req.Name, req.LocationType, req.WorkCenterId), ct);
        return CreatedAtAction(nameof(GetAll), new { }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStorageLocationRequest req, CancellationToken ct)
    {
        await mediator.Send(new UpdateStorageLocationCommand(id, req.Name, req.LocationType, req.WorkCenterId), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await mediator.Send(new DeleteStorageLocationCommand(id), ct);
        return NoContent();
    }
}

public record CreateStorageLocationRequest(string Code, string Name, LocationType LocationType, int? WorkCenterId = null);
public record UpdateStorageLocationRequest(string Name, LocationType LocationType, int? WorkCenterId = null);
