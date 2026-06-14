using AeroMes.Api.Auth;
using AeroMes.Application.Common;
using AeroMes.Application.Reports.Queries.GetDeliveryPerformance;
using AeroMes.Application.Reports.Queries.GetDowntimeReport;
using AeroMes.Application.Reports.Queries.GetInventorySnapshot;
using AeroMes.Application.Reports.Queries.GetOeeTrend;
using AeroMes.Application.Reports.Queries.GetOrderProgressReport;
using AeroMes.Application.Reports.Queries.GetOutputByEmployeeReport;
using AeroMes.Application.Reports.Queries.GetOutputByProductReport;
using AeroMes.Application.Reports.Queries.GetProductionReport;
using AeroMes.Application.Reports.Queries.GetQualityReport;
using AeroMes.Application.Reports.Queries.GetShiftProductionReport;
using AeroMes.Application.Reports.Queries.GetSoProductionStatusReport;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Domain.Master;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportsController(IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet("production")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<ApiResponse<ProductionReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<ProductionReportDto>>> GetProductionReport(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string? workCenterCode,
        [FromQuery] string? machineCode,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetProductionReportQuery(from, to, workCenterCode, machineCode), null, ct);
        return Ok(new ApiResponse<ProductionReportDto>(true, "OK", result));
    }

    [HttpGet("downtime")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<ApiResponse<DowntimeReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<DowntimeReportDto>>> GetDowntimeReport(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string? machineCode,
        [FromQuery] string? reasonCode,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetDowntimeReportQuery(from, to, machineCode, reasonCode), null, ct);
        return Ok(new ApiResponse<DowntimeReportDto>(true, "OK", result));
    }

    [HttpGet("quality")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<ApiResponse<QualityReportDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<QualityReportDto>>> GetQualityReport(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] string? defectCategory,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetQualityReportQuery(from, to, defectCategory), null, ct);
        return Ok(new ApiResponse<QualityReportDto>(true, "OK", result));
    }

    [HttpGet("production/order-progress")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<IReadOnlyList<OrderProgressDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderProgress(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? status,
        CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetOrderProgressReportQuery(from, to, status), null, ct));

    [HttpGet("production/output-by-employee")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<IReadOnlyList<EmployeeOutputDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOutputByEmployee(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetOutputByEmployeeReportQuery(from, to), null, ct));

    [HttpGet("production/output-by-product")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<IReadOnlyList<ProductOutputDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOutputByProduct(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetOutputByProductReportQuery(from, to), null, ct));

    [HttpGet("sales-orders/production-status")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<IReadOnlyList<SoProductionStatusDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSoProductionStatus(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetSoProductionStatusReportQuery(from, to), null, ct));

    [HttpGet("oee/trend")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<OeeTrendDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOeeTrend(
        [FromQuery] string machineCode,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] OeeTrendGranularity granularity = OeeTrendGranularity.Day,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetOeeTrendQuery(machineCode, from, to, granularity), null, ct));

    [HttpGet("delivery/performance")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<DeliveryPerformanceDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeliveryPerformance(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetDeliveryPerformanceQuery(from, to), null, ct));

    [HttpGet("inventory/snapshot")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<InventorySnapshotDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventorySnapshot(
        [FromQuery] LocationType? locationType,
        CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetInventorySnapshotQuery(locationType), null, ct));

    [HttpGet("shift/production")]
    [RequirePermission(Permissions.ReportRead)]
    [ProducesResponseType<ShiftProductionReportDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShiftProduction(
        [FromQuery] DateOnly shiftDate,
        [FromQuery] string? shiftCode,
        [FromQuery] int? workCenterId,
        CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetShiftProductionReportQuery(workCenterId, shiftDate, shiftCode), null, ct));
}
