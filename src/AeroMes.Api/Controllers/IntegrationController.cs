using AeroMes.Api.Auth;
using AeroMes.Application.Common;
using AeroMes.Application.Integration.Queries.GetProductionOrderDetail;
using AeroMes.Application.Integration.Queries.GetProductionOrders;
using AeroMes.Application.Integration.Queries.GetSalesOrderDetail;
using AeroMes.Application.Integration.Queries.GetSalesOrders;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/integration")]
[Authorize]
public class IntegrationController(IQueryMediator queryMediator) : ControllerBase
{
    // ── Sales Orders ─────────────────────────────────────────────────────────

    [HttpGet("sales-orders")]
    [RequirePermission(Permissions.IntegrationRead)]
    [ProducesResponseType<ApiResponse<IReadOnlyList<SalesOrderDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SalesOrderDto>>>> GetSalesOrders(
        [FromQuery] string? soCode,
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetSalesOrdersQuery(soCode, status, from, to), null, ct);
        return Ok(new ApiResponse<IReadOnlyList<SalesOrderDto>>(true, "OK", result));
    }

    [HttpGet("sales-orders/{id:int}")]
    [RequirePermission(Permissions.IntegrationRead)]
    [ProducesResponseType<ApiResponse<SalesOrderDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<SalesOrderDetailDto>>> GetSalesOrderById(
        int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetSalesOrderDetailQuery(id), null, ct);
        return Ok(new ApiResponse<SalesOrderDetailDto>(true, "OK", result));
    }

    // ── Production Orders ────────────────────────────────────────────────────

    [HttpGet("production-orders")]
    [RequirePermission(Permissions.IntegrationRead)]
    [ProducesResponseType<ApiResponse<IReadOnlyList<ProductionOrderDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProductionOrderDto>>>> GetProductionOrders(
        [FromQuery] int? soId,
        [FromQuery] string? poCode,
        [FromQuery] string? productCode,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetProductionOrdersQuery(soId, poCode, productCode, status), null, ct);
        return Ok(new ApiResponse<IReadOnlyList<ProductionOrderDto>>(true, "OK", result));
    }

    [HttpGet("production-orders/{id:int}")]
    [RequirePermission(Permissions.IntegrationRead)]
    [ProducesResponseType<ApiResponse<ProductionOrderDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<ProductionOrderDetailDto>>> GetProductionOrderById(
        int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetProductionOrderDetailQuery(id), null, ct);
        return Ok(new ApiResponse<ProductionOrderDetailDto>(true, "OK", result));
    }
}
