using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Production.Statistics.Commands.CreateProductionStatisticsSheet;
using AeroMes.Application.Production.Statistics.Commands.SubmitProductionStatisticsSheet;
using AeroMes.Application.Production.Statistics.Queries.GetProductionStatisticsSheetDetail;
using AeroMes.Application.Production.Statistics.Queries.GetProductionStatisticsSheets;
using AeroMes.Domain.Production;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/production-statistics")]
[Authorize]
public class ProductionStatisticsController(
    IQueryMediator queryMediator,
    ICommandMediator commandMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<IReadOnlyList<ProductionStatisticsSheetSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? poId,
        [FromQuery] int? mpoId,
        [FromQuery] StatisticsSheetType? sheetType,
        [FromQuery] StatisticsSheetStatus? status,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetProductionStatisticsSheetsQuery(poId, mpoId, sheetType, status, from, to), null, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<ProductionStatisticsSheetDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetProductionStatisticsSheetDetailQuery(id), null, ct);
        if (!result.IsFound) return NotFound(result.ErrorMessage);
        return Ok(result.Value!);
    }

    [HttpPost]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType<StatisticsSheetCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductionStatisticsSheetRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateProductionStatisticsSheetCommand(
                req.SheetType,
                req.POID,
                req.MPOId,
                req.ProductionDate,
                req.Notes,
                [.. req.OutputLines.Select(l => new OutputLineInput(
                    l.ProductCode, l.PlannedQty, l.QualifiedQty, l.DefectiveQty, l.DefectCodeId))],
                [.. req.MaterialLines.Select(l => new MaterialLineInput(
                    l.MaterialCode, l.BomStandardQty, l.ActualUsedQty, l.UoMCode, l.VarianceReason))],
                [.. req.ByProductLines.Select(l => new ByProductLineInput(
                    l.ProductCode, l.Qty, l.UoMCode, l.WarehouseCode))],
                User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created, result.Value!);
    }

    [HttpPost("{id:int}/submit")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Submit(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SubmitProductionStatisticsSheetCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record OutputLineRequest(
    string ProductCode,
    int PlannedQty,
    int QualifiedQty,
    int DefectiveQty,
    int? DefectCodeId);

public record MaterialLineRequest(
    string MaterialCode,
    decimal BomStandardQty,
    decimal ActualUsedQty,
    string UoMCode,
    string? VarianceReason);

public record ByProductLineRequest(
    string ProductCode,
    int Qty,
    string UoMCode,
    string? WarehouseCode);

public record CreateProductionStatisticsSheetRequest(
    StatisticsSheetType SheetType,
    int? POID,
    int? MPOId,
    DateOnly ProductionDate,
    string? Notes,
    IReadOnlyList<OutputLineRequest> OutputLines,
    IReadOnlyList<MaterialLineRequest> MaterialLines,
    IReadOnlyList<ByProductLineRequest> ByProductLines);
