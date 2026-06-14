using AeroMes.Api.Auth;
using AeroMes.Application.Interfaces;
using AeroMes.Application.Overview.Queries.GetDefectPareto;
using AeroMes.Application.Overview.Queries.GetFactoryKpi;
using AeroMes.Application.Overview.Queries.GetGrnTrend;
using AeroMes.Application.Overview.Queries.GetInventoryAlert;
using AeroMes.Application.Overview.Queries.GetMyDailyOutput;
using AeroMes.Application.Overview.Queries.GetMyOutputHistory;
using AeroMes.Application.Overview.Queries.GetOeeByMachine;
using AeroMes.Application.Overview.Queries.GetShiftOutputSummary;
using AeroMes.Application.Overview.Queries.GetSoFulfillmentRate;
using AeroMes.Application.Overview.Queries.GetStockByLocation;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
public class DashboardController(IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet("kpi")]
    [RequirePermission(Permissions.DashboardViewManager)]
    [ProducesResponseType<FactoryKpiDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFactoryKpi(
        [FromQuery] DateOnly? date, CancellationToken ct)
    {
        var d = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return Ok(await queryMediator.QueryAsync(new GetFactoryKpiQuery(d), null, ct));
    }

    [HttpGet("oee")]
    [RequirePermission(Permissions.DashboardViewManager)]
    [ProducesResponseType<IReadOnlyList<OeeByMachineDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOeeByMachine(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
    {
        var f = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var t = to   ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return Ok(await queryMediator.QueryAsync(new GetOeeByMachineQuery(f, t), null, ct));
    }

    [HttpGet("shift-output")]
    [RequirePermission(Permissions.DashboardViewManager)]
    [ProducesResponseType<IReadOnlyList<ShiftOutputDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShiftOutput(
        [FromQuery] DateOnly? date, CancellationToken ct)
    {
        var d = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return Ok(await queryMediator.QueryAsync(new GetShiftOutputSummaryQuery(d), null, ct));
    }

    [HttpGet("defect-pareto")]
    [RequirePermission(Permissions.DashboardViewManager)]
    [ProducesResponseType<IReadOnlyList<DefectParetoItemDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDefectPareto(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] string? productCode,
        CancellationToken ct)
    {
        var f = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var t = to   ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return Ok(await queryMediator.QueryAsync(new GetDefectParetoQuery(f, t, productCode), null, ct));
    }

    [HttpGet("inventory-alerts")]
    [RequirePermission(Permissions.DashboardViewManager)]
    [ProducesResponseType<InventoryAlertSummaryDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryAlerts(CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetInventoryAlertQuery(), null, ct));

    [HttpGet("so-fulfillment")]
    [RequirePermission(Permissions.DashboardViewManager)]
    [ProducesResponseType<SoFulfillmentDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSoFulfillment(
        [FromQuery] int? year, [FromQuery] int? month, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        return Ok(await queryMediator.QueryAsync(
            new GetSoFulfillmentRateQuery(year ?? now.Year, month ?? now.Month), null, ct));
    }

    [HttpGet("my/daily")]
    [ProducesResponseType<MyDailyOutputDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyDaily([FromQuery] DateOnly? date, CancellationToken ct)
    {
        var operatorId = User.Identity?.Name ?? string.Empty;
        var d = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return Ok(await queryMediator.QueryAsync(new GetMyDailyOutputQuery(operatorId, d), null, ct));
    }

    [HttpGet("my/history")]
    [ProducesResponseType<IReadOnlyList<MyDailyOutputDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyHistory([FromQuery] int days = 30, CancellationToken ct = default)
    {
        var operatorId = User.Identity?.Name ?? string.Empty;
        return Ok(await queryMediator.QueryAsync(new GetMyOutputHistoryQuery(operatorId, days), null, ct));
    }

    [HttpGet("stock-by-location")]
    [RequirePermission(Permissions.DashboardViewManager)]
    [ProducesResponseType<IReadOnlyList<StockByLocationDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStockByLocation(CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetStockByLocationQuery(), null, ct));

    [HttpGet("grn-trend")]
    [RequirePermission(Permissions.DashboardViewManager)]
    [ProducesResponseType<IReadOnlyList<GrnTrendDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGrnTrend(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
    {
        var f = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var t = to   ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return Ok(await queryMediator.QueryAsync(new GetGrnTrendQuery(f, t), null, ct));
    }
}
