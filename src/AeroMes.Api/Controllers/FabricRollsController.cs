using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Master.FabricRolls.Commands.ConsumeFabricFromRoll;
using AeroMes.Application.Master.FabricRolls.Commands.QuarantineFabricRoll;
using AeroMes.Application.Master.FabricRolls.Commands.RegisterFabricRoll;
using AeroMes.Application.Master.FabricRolls.Commands.ReserveFabricRolls;
using AeroMes.Application.Master.FabricRolls.Queries.GetFabricInventorySummary;
using AeroMes.Application.Master.FabricRolls.Queries.GetFabricRollDetail;
using AeroMes.Application.Master.FabricRolls.Queries.GetFabricRollHistory;
using AeroMes.Application.Master.FabricRolls.Queries.GetFabricRollsByShade;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/fabric-rolls")]
[Authorize]
public class FabricRollsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterFabricRollRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RegisterFabricRollCommand(
                request.RollBarcode,
                request.FabricProductCode,
                request.LotNumber,
                request.ShadeCode,
                request.GrossLengthMeters,
                request.GrossWeightKg,
                request.FabricWidthCm,
                request.SupplierCode,
                request.LocationID),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<FabricRollDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByShade(
        [FromQuery] string fabricProductCode,
        [FromQuery] string? shade = null,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetFabricRollsByShadeQuery(fabricProductCode, shade), null, ct));

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<FabricRollDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(int id, CancellationToken ct)
    {
        var dto = await queryMediator.QueryAsync(new GetFabricRollDetailQuery(id), null, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet("{id:int}/history")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<FabricConsumptionLogDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(int id, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetFabricRollHistoryQuery(id), null, ct));

    [HttpPost("reserve")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reserve([FromBody] ReserveFabricRollsRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ReserveFabricRollsCommand(request.CutOrderID, request.RollIDs), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPost("{id:int}/consume")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Consume(int id, [FromBody] ConsumeFabricRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ConsumeFabricFromRollCommand(id, request.CutOrderID, request.MetersConsumed, request.RecordedBy),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPost("{id:int}/quarantine")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Quarantine(int id, [FromBody] QuarantineFabricRollRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new QuarantineFabricRollCommand(id, request.Reason), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }
}

[ApiController]
[Route("api/v1/fabric-inventory")]
[Authorize]
public class FabricInventoryController(IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet("summary")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<FabricInventorySummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] string? fabricProductCode = null,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetFabricInventorySummaryQuery(fabricProductCode), null, ct));
}

public record RegisterFabricRollRequest(
    string RollBarcode,
    string FabricProductCode,
    string LotNumber,
    string ShadeCode,
    decimal GrossLengthMeters,
    decimal GrossWeightKg,
    decimal FabricWidthCm,
    string? SupplierCode,
    int? LocationID);

public record ReserveFabricRollsRequest(int CutOrderID, IReadOnlyList<int> RollIDs);
public record ConsumeFabricRequest(int CutOrderID, decimal MetersConsumed, string RecordedBy);
public record QuarantineFabricRollRequest(string Reason);
