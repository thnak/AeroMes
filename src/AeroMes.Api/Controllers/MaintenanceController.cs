using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Maintenance.Commands.AddMaintCostLine;
using AeroMes.Application.Maintenance.Commands.CompletePmWorkOrder;
using AeroMes.Application.Maintenance.Commands.CreateMaintenanceOrder;
using AeroMes.Application.Maintenance.Commands.StartPmWorkOrder;
using AeroMes.Application.Maintenance.Commands.UpdateMaintenanceOrderStatus;
using AeroMes.Application.Maintenance.Queries.GetMachineTco;
using AeroMes.Application.Maintenance.Queries.GetMaintenanceCalendar;
using AeroMes.Application.Maintenance.Queries.GetMaintenanceOrders;
using AeroMes.Domain.Maintenance;
using AeroMes.Domain.Maintenance.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/maintenance")]
[Authorize]
public class MaintenanceController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpPost("orders")]
    [RequirePermission(Permissions.MaintenanceWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMaintOrderRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateMaintenanceOrderCommand(
                request.MaintOrderCode, request.MachineCode, request.OrderType,
                request.TriggerRef, request.Priority, request.PlannedStartAt,
                request.PlannedEndAt, request.AssignedTo, request.EstimatedCost,
                request.Notes, User.Identity?.Name ?? "system"),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpGet("orders")]
    [RequirePermission(Permissions.MaintenanceRead)]
    [ProducesResponseType<PagedResult<MaintenanceOrderDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? machineCode,
        [FromQuery] MaintenanceOrderStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetMaintenanceOrdersQuery(machineCode, status, page, pageSize), null, ct));

    [HttpPost("orders/{id:int}/cost-lines")]
    [RequirePermission(Permissions.MaintenanceWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddCostLine(
        int id, [FromBody] AddCostLineRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AddMaintCostLineCommand(
                id, request.CostCategory,
                request.ProductCode, request.LotNumber, request.QtyUsed, request.UnitCost,
                request.OperatorID, request.LaborHours, request.LaborRateSnapshot,
                request.SupplierID, request.InvoiceRef, request.InvoiceAmount,
                User.Identity?.Name ?? "system"),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPost("orders/{id:int}/status")]
    [RequirePermission(Permissions.MaintenanceWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        int id, [FromBody] UpdateMaintStatusRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateMaintenanceOrderStatusCommand(id, request.Action, User.Identity?.Name),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpGet("tco/{machineCode}")]
    [RequirePermission(Permissions.MaintenanceRead)]
    [ProducesResponseType<IReadOnlyList<MachineTcoDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTco(
        string machineCode, [FromQuery] int months = 12, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetMachineTcoQuery(machineCode, months), null, ct));

    [HttpGet("calendar")]
    [RequirePermission(Permissions.MaintenanceRead)]
    [ProducesResponseType<IReadOnlyList<MwoCalendarDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalendar(
        [FromQuery] DateTime from, [FromQuery] DateTime to,
        [FromQuery] string? machineCode = null, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetMaintenanceCalendarQuery(from, to, machineCode), null, ct));

    [HttpPost("pm/{id:int}/start")]
    [RequirePermission(Permissions.MaintenanceWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartPmWorkOrder(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new StartPmWorkOrderCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPost("pm/{id:int}/complete")]
    [RequirePermission(Permissions.MaintenanceWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompletePmWorkOrder(
        int id, [FromBody] CompletePmWorkOrderRequest request, CancellationToken ct)
    {
        var user = User.Identity?.Name ?? "system";
        var result = await commandMediator.SendAsync(
            new CompletePmWorkOrderCommand(id, user, request.ChecklistResults), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }
}

public record CreateMaintOrderRequest(
    string MaintOrderCode,
    string MachineCode,
    MaintenanceOrderType OrderType,
    string? TriggerRef,
    MaintenancePriority Priority,
    DateTime? PlannedStartAt,
    DateTime? PlannedEndAt,
    string? AssignedTo,
    decimal? EstimatedCost,
    string? Notes);

public record AddCostLineRequest(
    CostCategory CostCategory,
    string? ProductCode,
    string? LotNumber,
    decimal? QtyUsed,
    decimal? UnitCost,
    string? OperatorID,
    decimal? LaborHours,
    decimal? LaborRateSnapshot,
    int? SupplierID,
    string? InvoiceRef,
    decimal? InvoiceAmount);

public record UpdateMaintStatusRequest(string Action);
public record CompletePmWorkOrderRequest(IReadOnlyList<ChecklistResultInput> ChecklistResults);
