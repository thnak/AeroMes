using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Wms.Commands.CreateAisle;
using AeroMes.Application.Wms.Commands.DeleteAisle;
using AeroMes.Application.Wms.Queries.GetAisles;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/warehouse/aisles")]
[Authorize]
public class WarehouseAislesController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.WarehouseRead)]
    [ProducesResponseType<IReadOnlyList<AisleDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int zoneId, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetAislesQuery(zoneId), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<AisleCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateAisleRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateAisleCommand(req.ZoneId, req.AisleCode, req.PickSequence, User.Identity?.Name),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetAll), new { zoneId = req.ZoneId }, new AisleCreatedResult(result.Value!));
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteAisleCommand(id, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateAisleRequest(int ZoneId, string AisleCode, int PickSequence);
public record AisleCreatedResult(int AisleId);
