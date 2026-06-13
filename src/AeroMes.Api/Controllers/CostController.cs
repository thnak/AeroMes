using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Cost.Commands.CloseReworkOrder;
using AeroMes.Application.Cost.Commands.CreateReworkOrder;
using AeroMes.Application.Cost.Commands.PostScrap;
using AeroMes.Application.Cost.Queries.GetCopqTrend;
using AeroMes.Application.Cost.Queries.GetQualityCostSummary;
using AeroMes.Application.Cost.Queries.GetReworkOrders;
using AeroMes.Application.Cost.Queries.GetScrapPareto;
using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/cost")]
[Authorize]
public class CostController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpPost("scrap")]
    [RequirePermission(Permissions.CostWrite)]
    [ProducesResponseType<long>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostScrap([FromBody] PostScrapRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new PostScrapCommand(
                request.WOID, request.LogID, request.DefectCodeId,
                request.ProductCode, request.LotNumber, request.ScrapQty,
                request.MaterialCostPerUnit, request.LaborCostSunk, request.MachineCostSunk,
                request.DisposalMethod, request.ScrapLocationId, request.ApprovedBy,
                request.Notes, User.Identity?.Name),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpGet("scrap/pareto")]
    [RequirePermission(Permissions.CostRead)]
    [ProducesResponseType<IReadOnlyList<ScrapParetoDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetScrapPareto(
        [FromQuery] DateTime from, [FromQuery] DateTime to,
        [FromQuery] int? workCenterId, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(
            new GetScrapParetoQuery(from, to, workCenterId), null, ct));

    [HttpPost("rework-orders")]
    [RequirePermission(Permissions.CostWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateReworkOrder(
        [FromBody] CreateReworkOrderRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateReworkOrderCommand(
                request.ReworkCode, request.SourceWOID, request.ScrapTxID,
                request.ProductCode, request.ReworkQty, request.ReworkStepFromId,
                User.Identity?.Name ?? "system"),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpPost("rework-orders/{id:int}/complete")]
    [RequirePermission(Permissions.CostWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteReworkOrder(
        int id, [FromBody] CloseReworkOrderRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CloseReworkOrderCommand(id, request.ActMaterialCost, request.ActLaborCost,
                request.ActMachineCost, User.Identity?.Name),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpGet("rework-orders")]
    [RequirePermission(Permissions.CostRead)]
    [ProducesResponseType<PagedResult<ReworkOrderDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReworkOrders(
        [FromQuery] ReworkStatus? status,
        [FromQuery] string? productCode,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetReworkOrdersQuery(status, productCode, page, pageSize), null, ct));

    [HttpGet("quality-cost-summary")]
    [RequirePermission(Permissions.CostRead)]
    [ProducesResponseType<IReadOnlyList<QualityCostSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQualityCostSummary(
        [FromQuery] short year, [FromQuery] byte? month, [FromQuery] string? productCode,
        CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(
            new GetQualityCostSummaryQuery(year, month, productCode), null, ct));

    [HttpGet("copq-trend")]
    [RequirePermission(Permissions.CostRead)]
    [ProducesResponseType<IReadOnlyList<CopqTrendPointDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCopqTrend(
        [FromQuery] int months = 12, [FromQuery] string? productCode = null,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetCopqTrendQuery(months, productCode), null, ct));
}

public record PostScrapRequest(
    int WOID,
    long? LogID,
    int? DefectCodeId,
    string ProductCode,
    string? LotNumber,
    int ScrapQty,
    decimal MaterialCostPerUnit,
    decimal LaborCostSunk,
    decimal MachineCostSunk,
    DisposalMethod DisposalMethod,
    int? ScrapLocationId,
    string? ApprovedBy,
    string? Notes);

public record CreateReworkOrderRequest(
    string ReworkCode,
    int SourceWOID,
    long? ScrapTxID,
    string ProductCode,
    int ReworkQty,
    int? ReworkStepFromId);

public record CloseReworkOrderRequest(
    decimal ActMaterialCost,
    decimal ActLaborCost,
    decimal ActMachineCost);
