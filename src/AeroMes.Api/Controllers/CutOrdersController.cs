using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Production.Commands.CompleteCutting;
using AeroMes.Application.Production.Commands.CreateCutOrder;
using AeroMes.Application.Production.Commands.ReserveFabricForCutOrder;
using AeroMes.Application.Production.Commands.StartCutting;
using AeroMes.Application.Production.Queries.GetCutOrder;
using AeroMes.Application.Production.Queries.GetCutOrdersByWO;
using AeroMes.Application.Production.Queries.GetMarkerEfficiencyReport;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/cut-orders")]
[Authorize]
public class CutOrdersController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpPost]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCutOrderRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateCutOrderCommand(
                request.CutOrderCode, request.WOID, request.StyleCode, request.ColorCode,
                request.FabricProductCode, request.ShadeCode, request.PlyCount,
                request.SpreadLengthMeters, request.FabricWidthCm,
                request.Lines.Select(l => new CutOrderLineInput(l.SizeCode, l.QuantityToCut)).ToList(),
                request.MarkerReference, request.EstimatedFabricMeters),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpGet]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<IReadOnlyList<CutOrderDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByWO([FromQuery] int woid, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetCutOrdersByWOQuery(woid), null, ct));

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<CutOrderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(int id, CancellationToken ct)
    {
        var dto = await queryMediator.QueryAsync(new GetCutOrderQuery(id), null, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:int}/reserve-fabric")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReserveFabric(int id, [FromBody] ReserveFabricRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ReserveFabricForCutOrderCommand(id, request.RollIDs), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPost("{id:int}/start")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartCutting(int id, [FromBody] StartCuttingRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new StartCuttingCommand(id, request.OperatorID), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPost("{id:int}/complete")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteCutting(int id, [FromBody] CompleteCuttingRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CompleteCuttingCommand(
                id, request.ActualFabricMeters, request.MarkerEfficiencyPct,
                request.Lines.Select(l => new CompleteCuttingLineInput(l.SizeCode, l.QuantityCut)).ToList(),
                request.BundleSize),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpGet("marker-efficiency")]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<IReadOnlyList<MarkerEfficiencyReportDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMarkerEfficiency(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? styleCode,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetMarkerEfficiencyReportQuery(fromDate, toDate, styleCode), null, ct));
}

public record CutOrderLineRequestItem(string SizeCode, int QuantityToCut);
public record CompleteCuttingLineRequestItem(string SizeCode, int QuantityCut);

public record CreateCutOrderRequest(
    string CutOrderCode,
    int WOID,
    string StyleCode,
    string ColorCode,
    string FabricProductCode,
    string ShadeCode,
    int PlyCount,
    decimal SpreadLengthMeters,
    decimal FabricWidthCm,
    IReadOnlyList<CutOrderLineRequestItem> Lines,
    string? MarkerReference = null,
    decimal? EstimatedFabricMeters = null);

public record ReserveFabricRequest(IReadOnlyList<int> RollIDs);
public record StartCuttingRequest(string OperatorID);
public record CompleteCuttingRequest(
    decimal ActualFabricMeters,
    decimal MarkerEfficiencyPct,
    IReadOnlyList<CompleteCuttingLineRequestItem> Lines,
    int BundleSize = 12);
