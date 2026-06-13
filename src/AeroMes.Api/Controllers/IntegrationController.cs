using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Integration.Commands.SaveErpSettings;
using AeroMes.Application.Integration.Commands.SyncProductionOrders;
using AeroMes.Application.Integration.Commands.BatchCreateProductionOrders;
using AeroMes.Application.Integration.Commands.SyncSalesOrders;
using AeroMes.Application.Integration.Commands.TestErpConnection;
using AeroMes.Application.Integration.Queries.GetErpSettings;
using AeroMes.Application.Integration.Queries.GetProductionOrderDetail;
using AeroMes.Application.Integration.Queries.GetProductionOrders;
using AeroMes.Application.Integration.Queries.GetSalesOrderDetail;
using AeroMes.Application.Integration.Queries.GetSalesOrders;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/integration")]
[Authorize]
public class IntegrationController(
    IQueryMediator queryMediator,
    ICommandMediator commandMediator) : ControllerBase
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
        if (!result.IsFound) return NotFound(result.ErrorMessage);
        return Ok(new ApiResponse<SalesOrderDetailDto>(true, "OK", result.Value!));
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
        if (!result.IsFound) return NotFound(result.ErrorMessage);
        return Ok(new ApiResponse<ProductionOrderDetailDto>(true, "OK", result.Value!));
    }

    // ── ERP Settings ─────────────────────────────────────────────────────────

    [HttpGet("erp-settings")]
    [RequirePermission(Permissions.IntegrationConfigure)]
    [ProducesResponseType<ApiResponse<ErpSettingsDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ErpSettingsDto>>> GetErpSettings(CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetErpSettingsQuery(), null, ct);
        return Ok(new ApiResponse<ErpSettingsDto>(true, "OK", result));
    }

    [HttpPut("erp-settings")]
    [RequirePermission(Permissions.IntegrationConfigure)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveErpSettings(
        [FromBody] SaveErpSettingsRequest body, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SaveErpSettingsCommand(body.ErpEnabled, body.ErpBaseUrl, body.ErpApiKey, body.ErpSyncIntervalMinutes),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // ── Sync ─────────────────────────────────────────────────────────────────

    [HttpPost("sync/sales-orders")]
    [RequirePermission(Permissions.IntegrationSync)]
    [ProducesResponseType<ApiResponse<ErpSyncResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ErpSyncResult>>> SyncSalesOrders(CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new SyncSalesOrdersCommand(), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(new ApiResponse<ErpSyncResult>(true, "Sync complete", result.Value!));
    }

    [HttpPost("sync/production-orders")]
    [RequirePermission(Permissions.IntegrationSync)]
    [ProducesResponseType<ApiResponse<ErpSyncResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ErpSyncResult>>> SyncProductionOrders(CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new SyncProductionOrdersCommand(), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(new ApiResponse<ErpSyncResult>(true, "Sync complete", result.Value!));
    }

    // ── Batch Production Order Creation ──────────────────────────────────────

    [HttpPost("production-orders/batch")]
    [RequirePermission(Permissions.IntegrationSync)]
    [ProducesResponseType<BatchCreateResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> BatchCreateProductionOrders(
        [FromBody] BatchCreateProductionOrdersRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new BatchCreateProductionOrdersCommand(
                [.. req.Items.Select(i => new BatchOrderItem(
                    i.ProductCode, i.TargetQuantity,
                    i.PlannedStartDate, i.PlannedEndDate, i.ProductionDeadline,
                    i.Priority, i.AssignedTo))],
                User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created, result.Value!);
    }

    [HttpPost("test-connection")]
    [RequirePermission(Permissions.IntegrationConfigure)]
    [ProducesResponseType<ApiResponse<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> TestConnection(CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new TestErpConnectionCommand(), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(new ApiResponse<bool>(true, "Connection successful", true));
    }
}

public record SaveErpSettingsRequest(
    bool ErpEnabled,
    string? ErpBaseUrl,
    string? ErpApiKey,
    int ErpSyncIntervalMinutes);

public record BatchOrderItemRequest(
    string ProductCode,
    int TargetQuantity,
    DateTime? PlannedStartDate,
    DateTime? PlannedEndDate,
    DateTime? ProductionDeadline,
    byte Priority,
    string? AssignedTo);

public record BatchCreateProductionOrdersRequest(IReadOnlyList<BatchOrderItemRequest> Items);
