using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Quality.Commands.RecordAQLInspection;
using AeroMes.Application.Quality.Commands.RecordInlineInspection;
using AeroMes.Application.Quality.Queries.GetDefectPareto;
using AeroMes.Application.Quality.Queries.GetDHUTrend;
using AeroMes.Application.Quality.Queries.GetQualitySummaryByStyle;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/quality")]
[Authorize]
public class QualityInspectionController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator,
    IConfiguration configuration) : ControllerBase
{
    private bool IsDHUEnabled =>
        configuration.GetValue<bool>("Features:Apparel:DHU_Quality", defaultValue: false);

    [HttpPost("inline-inspections")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType<long>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordInlineInspection(
        [FromBody] RecordInlineInspectionRequest request, CancellationToken ct)
    {
        if (!IsDHUEnabled) return Forbid();
        var result = await commandMediator.SendAsync(
            new RecordInlineInspectionCommand(
                request.WOID, request.WorkCenterID, request.StyleCode, request.ColorCode,
                request.InspectorID, request.ShiftCode, request.SampleSize,
                request.Defects.Select(d =>
                    new InlineInspectionDefectInput(d.DefectCode, d.Quantity, d.DefectLocation, d.IsMajor))
                    .ToList(),
                request.DHU_Target, request.Notes),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpPost("aql-inspections")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordAQLInspection(
        [FromBody] RecordAQLInspectionRequest request, CancellationToken ct)
    {
        if (!IsDHUEnabled) return Forbid();
        var result = await commandMediator.SendAsync(
            new RecordAQLInspectionCommand(
                request.WOID, request.AQLLevel, request.InspectionLevel, request.LotSize,
                request.InspectorID,
                request.Defects.Select(d => new AQLDefectInput(d.DefectCode, d.Quantity, d.IsMajor)).ToList(),
                request.Notes),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpGet("dhu-trend")]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<IReadOnlyList<DHUTrendDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDHUTrend(
        [FromQuery] int? woid,
        [FromQuery] int? workCenterId,
        [FromQuery] string? styleCode,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        if (!IsDHUEnabled) return Forbid();
        var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
        var toDate = to ?? DateTime.UtcNow;
        return Ok(await queryMediator.QueryAsync(
            new GetDHUTrendQuery(woid, workCenterId, styleCode, fromDate, toDate), null, ct));
    }

    [HttpGet("defect-pareto")]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<IReadOnlyList<DefectParetoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDefectPareto(
        [FromQuery] string? styleCode,
        [FromQuery] int? workCenterId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int topN = 10,
        CancellationToken ct = default)
    {
        if (!IsDHUEnabled) return Forbid();
        var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
        var toDate = to ?? DateTime.UtcNow;
        return Ok(await queryMediator.QueryAsync(
            new GetDefectParetoQuery(styleCode, workCenterId, fromDate, toDate, topN), null, ct));
    }

    [HttpGet("summary")]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<QualitySummaryByStyleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] string styleCode,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        if (!IsDHUEnabled) return Forbid();
        var fromDate = from ?? DateTime.UtcNow.AddDays(-90);
        var toDate = to ?? DateTime.UtcNow;
        var dto = await queryMediator.QueryAsync(
            new GetQualitySummaryByStyleQuery(styleCode, fromDate, toDate), null, ct);
        return dto is null ? NotFound() : Ok(dto);
    }
}

public record InlineDefectRequestItem(
    string DefectCode,
    int Quantity,
    string? DefectLocation,
    bool IsMajor);

public record RecordInlineInspectionRequest(
    int WOID,
    int WorkCenterID,
    string StyleCode,
    string? ColorCode,
    string InspectorID,
    string ShiftCode,
    int SampleSize,
    IReadOnlyList<InlineDefectRequestItem> Defects,
    decimal DHU_Target = 2.5m,
    string? Notes = null);

public record AQLDefectRequestItem(string DefectCode, int Quantity, bool IsMajor);

public record RecordAQLInspectionRequest(
    int WOID,
    string AQLLevel,
    string InspectionLevel,
    int LotSize,
    string InspectorID,
    IReadOnlyList<AQLDefectRequestItem> Defects,
    string? Notes = null);
