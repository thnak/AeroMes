using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Wms.Commands.ActivateBin;
using AeroMes.Application.Wms.Commands.CreateBin;
using AeroMes.Application.Wms.Commands.DeactivateBin;
using AeroMes.Application.Wms.Commands.DeleteBin;
using AeroMes.Application.Wms.Commands.UpdateBin;
using AeroMes.Application.Wms.Queries.GetBinStock;
using AeroMes.Application.Wms.Queries.GetBins;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/warehouse/bins")]
[Authorize]
public class WarehouseBinsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.WarehouseRead)]
    [ProducesResponseType<IReadOnlyList<BinDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int rackId,
        [FromQuery] bool activeOnly = true,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetBinsQuery(rackId, activeOnly), null, ct));

    [HttpGet("{id:int}/stock")]
    [RequirePermission(Permissions.InventoryRead)]
    [ProducesResponseType<IReadOnlyList<BinStockDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStock(int id, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetBinStockQuery(id), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<BinCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateBinRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateBinCommand(req.RackId, req.BinCode, req.BinLevel, req.BinType, req.MaxQty, User.Identity?.Name),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetAll), new { rackId = req.RackId }, new BinCreatedResult(result.Value!));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBinRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateBinCommand(id, req.BinLevel, req.BinType, req.MaxQty, User.Identity?.Name ?? "system"),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("{id:int}/activate")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ActivateBinCommand(id, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("{id:int}/deactivate")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeactivateBinCommand(id, User.Identity?.Name ?? "system"), null, ct);
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
            new DeleteBinCommand(id, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateBinRequest(int RackId, string BinCode, string BinLevel, BinType BinType, decimal? MaxQty);
public record UpdateBinRequest(string BinLevel, BinType BinType, decimal? MaxQty);
public record BinCreatedResult(int BinId);
