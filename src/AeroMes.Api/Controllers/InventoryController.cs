using AeroMes.Api.Auth;
using AeroMes.Application.Common;
using AeroMes.Application.Inventory.Queries.GetInventoryStock;
using AeroMes.Application.Inventory.Queries.GetLotTrace;
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
}
