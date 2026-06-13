using AeroMes.Api.Auth;
using AeroMes.Application.Common;
using AeroMes.Application.Reports.Queries.GetDowntimeReport;
using AeroMes.Application.Reports.Queries.GetProductionReport;
using AeroMes.Application.Reports.Queries.GetQualityReport;
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
}
