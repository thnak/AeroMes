using AeroMes.Api.Auth;
using AeroMes.Application.Common;
using AeroMes.Application.Inventory.Queries.GetInventoryStock;
using AeroMes.Application.Inventory.Queries.GetLotTrace;
using AeroMes.Application.Inventory.Queries.GetStockMovements;
using AeroMes.Application.Production.Queries.GetAvailableStock;
using AeroMes.Application.Production.Queries.GetInventoryByExpiry;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/inventory")]
[Authorize]
public class InventoryController(IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.InventoryRead)]
    [ProducesResponseType<ApiResponse<IReadOnlyList<InventoryStockDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InventoryStockDto>>>> GetStock(
        [FromQuery] string? locationType,
        [FromQuery] string? productCode,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetInventoryStockQuery(locationType, productCode), null, ct);
        return Ok(new ApiResponse<IReadOnlyList<InventoryStockDto>>(true, "OK", result));
    }

    [HttpGet("trace/{lotNumber}")]
    [RequirePermission(Permissions.InventoryRead)]
    [ProducesResponseType<ApiResponse<LotTraceDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<LotTraceDto>>> TraceLot(
        string lotNumber, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetLotTraceQuery(lotNumber), null, ct);
        return Ok(new ApiResponse<LotTraceDto>(true, "OK", result));
    }

    [HttpGet("available")]
    [RequirePermission(Permissions.InventoryRead)]
    [ProducesResponseType<IReadOnlyList<AvailableStockDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableStock(
        [FromQuery] string productCode,
        [FromQuery] int? locationId,
        CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetAvailableStockQuery(productCode, locationId), null, ct));

    [HttpGet("expiring")]
    [RequirePermission(Permissions.InventoryRead)]
    [ProducesResponseType<IReadOnlyList<ExpiringStockDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiringStock(
        [FromQuery] int daysToExpiry = 30,
        [FromQuery] int? locationId = null,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetInventoryByExpiryQuery(daysToExpiry, locationId), null, ct));

    [HttpGet("stock-movements")]
    [RequirePermission(Permissions.InventoryRead)]
    [ProducesResponseType<IReadOnlyList<StockMovementDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStockMovements(
        [FromQuery] string? productCode,
        [FromQuery] string? lotNumber,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetStockMovementsQuery(productCode, lotNumber, page, pageSize), null, ct));
}
