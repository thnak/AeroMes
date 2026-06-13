using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Production.Commands.CloseBundleOperation;
using AeroMes.Application.Production.Commands.CreateBundlesFromCutOrder;
using AeroMes.Application.Production.Commands.ReceiveBundleAtStation;
using AeroMes.Application.Production.Commands.ReworkBundle;
using AeroMes.Application.Production.Queries.GetBundleLocation;
using AeroMes.Application.Production.Queries.GetLineBalancingReport;
using AeroMes.Application.Production.Queries.GetOperatorEfficiencyReport;
using AeroMes.Application.Production.Queries.GetWIPByStyle;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/bundles")]
[Authorize]
public class BundlesController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpPost("create-from-cut-order")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateFromCutOrder(
        [FromBody] CreateBundlesFromCutOrderRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateBundlesFromCutOrderCommand(request.CutOrderID, request.BundleSizePerBundle), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpGet("{barcode}")]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<BundleLocationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLocation(string barcode, CancellationToken ct)
    {
        var dto = await queryMediator.QueryAsync(new GetBundleLocationQuery(barcode), null, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{barcode}/receive")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Receive(string barcode, [FromBody] ReceiveBundleRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ReceiveBundleAtStationCommand(barcode, request.OperationCode, request.WorkCenterID, request.OperatorID),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPost("{barcode}/close")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Close(string barcode, [FromBody] CloseBundleRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CloseBundleOperationCommand(barcode, request.QtyOK, request.QtyNG, request.DefectCodes),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPost("{id:int}/rework")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rework(int id, [FromBody] ReworkBundleRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ReworkBundleCommand(id, request.TargetOperationCode, request.Reason), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpGet("wip")]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<IReadOnlyList<WIPByStyleDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWIP(
        [FromQuery] string styleCode,
        [FromQuery] string? colorCode = null,
        [FromQuery] int? woid = null,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetWIPByStyleQuery(styleCode, colorCode, woid), null, ct));

    [HttpGet("line-balance")]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<IReadOnlyList<LineBalancingDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLineBalance(
        [FromQuery] int? workCenterId = null,
        [FromQuery] string? styleCode = null,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetLineBalancingReportQuery(workCenterId, styleCode), null, ct));

    [HttpGet("operator-efficiency")]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<IReadOnlyList<OperatorEfficiencyDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOperatorEfficiency(
        [FromQuery] string? operatorId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetOperatorEfficiencyReportQuery(operatorId, fromDate, toDate), null, ct));
}

public record CreateBundlesFromCutOrderRequest(int CutOrderID, int BundleSizePerBundle = 12);
public record ReceiveBundleRequest(string OperationCode, int WorkCenterID, string OperatorID);
public record CloseBundleRequest(int QtyOK, int QtyNG, string? DefectCodes = null);
public record ReworkBundleRequest(string TargetOperationCode, string Reason);
