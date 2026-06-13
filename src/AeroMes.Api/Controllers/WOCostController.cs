using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Cost.WOCosts.Commands.CloseJobAndPostCost;
using AeroMes.Application.Cost.WOCosts.Commands.CloseWOAndComputeVariance;
using AeroMes.Application.Cost.WOCosts.Commands.PostMaterialConsumptionCost;
using AeroMes.Application.Cost.WOCosts.Queries.GetVarianceReport;
using AeroMes.Application.Cost.WOCosts.Queries.GetWOCostBreakdown;
using AeroMes.Domain.Cost.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/cost")]
[Authorize]
public class WOCostController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpGet("work-orders/{woId:int}/cost-summary")]
    [RequirePermission(Permissions.CostRead)]
    [ProducesResponseType<WOCostBreakdownResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCostSummary(int woId, [FromQuery] string? type, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetWOCostBreakdownQuery(woId, type), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("variance-report")]
    [RequirePermission(Permissions.CostRead)]
    [ProducesResponseType<PagedResult<VarianceReportItemDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVarianceReport(
        [FromQuery] string? productCode,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int? workCenterId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetVarianceReportQuery(productCode, from, to, workCenterId, page, pageSize), null, ct));

    [HttpPost("work-orders/{woId:int}/material-cost")]
    [RequirePermission(Permissions.CostWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PostMaterialCost(
        int woId, [FromBody] PostMaterialCostRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new PostMaterialConsumptionCostCommand(
                woId, request.ConsumptionID, request.ProductCode, request.LotNumber,
                request.QtyConsumed, request.ActualUnitCost), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPost("work-orders/{woId:int}/close-job")]
    [RequirePermission(Permissions.CostWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CloseJobAndPostCost(
        int woId, [FromBody] CloseJobCostRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CloseJobAndPostCostCommand(
                woId, request.JobID, request.MachineCode, request.OperatorID,
                request.LaborGradeID, request.ActualHours, request.HourlyRateSnapshot,
                request.IsOvertime, request.OvertimeMultiplierSnapshot,
                request.RuntimeHours, request.EnergyKWh, request.TotalMachineRateSnapshot), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPost("work-orders/{woId:int}/close")]
    [RequirePermission(Permissions.CostWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CloseWO(
        int woId, [FromBody] CloseWORequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CloseWOAndComputeVarianceCommand(
                woId, request.QtyOK, request.ScrapQty,
                request.StdMaterialCost, request.StdLaborCost, request.StdMachineCost,
                request.StdMaterialQty), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }
}

public record PostMaterialCostRequest(
    long? ConsumptionID, string ProductCode, string? LotNumber,
    decimal QtyConsumed, decimal ActualUnitCost);

public record CloseJobCostRequest(
    long JobID, string MachineCode, string OperatorID, int LaborGradeID,
    decimal ActualHours, decimal HourlyRateSnapshot, bool IsOvertime,
    decimal OvertimeMultiplierSnapshot, decimal RuntimeHours,
    decimal? EnergyKWh, decimal TotalMachineRateSnapshot);

public record CloseWORequest(
    int QtyOK, int ScrapQty,
    decimal StdMaterialCost, decimal StdLaborCost, decimal StdMachineCost,
    decimal StdMaterialQty);
