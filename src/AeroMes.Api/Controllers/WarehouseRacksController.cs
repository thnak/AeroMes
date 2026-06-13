using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Wms.Commands.CreateRack;
using AeroMes.Application.Wms.Commands.DeleteRack;
using AeroMes.Application.Wms.Queries.GetRacks;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/warehouse/racks")]
[Authorize]
public class WarehouseRacksController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.WarehouseRead)]
    [ProducesResponseType<IReadOnlyList<RackDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int aisleId, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetRacksQuery(aisleId), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<RackCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateRackRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateRackCommand(req.AisleId, req.RackCode, req.MaxWeightKg, User.Identity?.Name),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetAll), new { aisleId = req.AisleId }, new RackCreatedResult(result.Value!));
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteRackCommand(id, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateRackRequest(int AisleId, string RackCode, decimal? MaxWeightKg);
public record RackCreatedResult(int RackId);
