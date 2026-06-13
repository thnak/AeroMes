using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Master.Tools.Commands.AddToolOperation;
using AeroMes.Application.Master.Tools.Commands.CheckoutTool;
using AeroMes.Application.Master.Tools.Commands.DeleteTool;
using AeroMes.Application.Master.Tools.Commands.RecordToolMaintenance;
using AeroMes.Application.Master.Tools.Commands.RecordToolUsage;
using AeroMes.Application.Master.Tools.Commands.RegisterTool;
using AeroMes.Application.Master.Tools.Commands.RemoveToolOperation;
using AeroMes.Application.Master.Tools.Commands.ReturnTool;
using AeroMes.Application.Master.Tools.Commands.ScrapTool;
using AeroMes.Application.Master.Tools.Commands.SendToolForService;
using AeroMes.Application.Master.Tools.Commands.UpdateTool;
using AeroMes.Application.Master.Tools.Queries.GetToolByCode;
using AeroMes.Application.Master.Tools.Queries.GetTools;
using AeroMes.Application.Master.Tools.Queries.GetToolsDueForCalibration;
using AeroMes.Application.Master.Tools.Queries.GetToolsDueForReconditioning;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/tools")]
[Authorize]
public class ToolsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<ToolDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool activeOnly = true,
        [FromQuery] ToolType? toolType = null,
        [FromQuery] ToolStatus? status = null,
        [FromQuery] int? workCenterId = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetToolsQuery(activeOnly, toolType, status, workCenterId, search), null, ct));

    [HttpGet("due-calibration")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<ToolCalibrationDueDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDueForCalibration(
        [FromQuery] int withinDays = 7, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetToolsDueForCalibrationQuery(withinDays), null, ct));

    [HttpGet("due-reconditioning")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<ToolReconditioningDueDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDueForReconditioning(CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetToolsDueForReconditioningQuery(), null, ct));

    [HttpGet("{code}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<ToolDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetToolByCodeQuery(code), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<ToolCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register([FromBody] RegisterToolRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RegisterToolCommand(
                req.Code, req.Name, req.ToolType, req.Brand, req.Model, req.Specification,
                req.MaxUsageCount, req.PmIntervalCount,
                req.RequiresCalibration, req.CalibrationIntervalDays,
                req.StorageLocation, req.PurchaseDate, req.PurchaseCost,
                req.Notes, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        var code = result.Value!;
        return CreatedAtAction(nameof(GetByCode), new { code }, new ToolCreatedResult(code));
    }

    [HttpPut("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateToolRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateToolCommand(
                code, req.Name, req.ToolType, req.Brand, req.Model, req.Specification,
                req.MaxUsageCount, req.PmIntervalCount,
                req.RequiresCalibration, req.CalibrationIntervalDays,
                req.StorageLocation, req.PurchaseDate, req.PurchaseCost,
                req.Notes, req.IsActive, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(string code, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new DeleteToolCommand(code, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }

    // ── Operation mappings ──────────────────────────────────────────────────

    [HttpPost("{code}/operations")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<ToolOperationAddedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddOperation(string code, [FromBody] AddToolOperationRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AddToolOperationCommand(
                code, req.OperationCode, req.ProductCode, req.IsRequired, req.UsageCountPerOp,
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetByCode), new { code }, new ToolOperationAddedResult(result.Value!));
    }

    [HttpDelete("{code}/operations/{mappingId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveOperation(string code, int mappingId, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new RemoveToolOperationCommand(code, mappingId, User.Identity?.Name), null, ct);
        return NoContent();
    }

    // ── Checkout / return ───────────────────────────────────────────────────

    [HttpPost("{code}/checkout")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<ToolCheckedOutResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Checkout(string code, [FromBody] CheckoutToolRequest req, CancellationToken ct)
    {
        var checkedOutBy = req.CheckedOutBy ?? User.Identity?.Name ?? "system";
        var result = await commandMediator.SendAsync(
            new CheckoutToolCommand(code, req.WorkCenterId, checkedOutBy, req.ExpectedReturnAt, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetByCode), new { code }, new ToolCheckedOutResult(result.Value!));
    }

    [HttpPost("{code}/return")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Return(string code, [FromBody] ReturnToolRequest req, CancellationToken ct)
    {
        var returnedBy = req.ReturnedBy ?? User.Identity?.Name ?? "system";
        var result = await commandMediator.SendAsync(
            new ReturnToolCommand(code, returnedBy, req.Condition, req.Notes, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // ── Service, maintenance & usage ────────────────────────────────────────

    [HttpPost("{code}/service")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SendForService(string code, [FromBody] SendToolServiceRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SendToolForServiceCommand(code, req.ServiceType, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{code}/maintenance")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<ToolMaintenanceLoggedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RecordMaintenance(string code, [FromBody] RecordToolMaintenanceRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RecordToolMaintenanceCommand(
                code, req.MaintenanceType, req.PerformedAt, req.PerformedBy,
                req.Cost, req.NextDueCount, req.NextDueDate, req.Notes,
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetByCode), new { code }, new ToolMaintenanceLoggedResult(result.Value!));
    }

    [HttpPost("{code}/usage")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<RecordToolUsageResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RecordUsage(string code, [FromBody] RecordToolUsageRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RecordToolUsageCommand(code, req.Count, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value!);
    }

    [HttpPost("{code}/scrap")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Scrap(string code, CancellationToken ct)
    {
        await commandMediator.SendAsync(new ScrapToolCommand(code, User.Identity?.Name), null, ct);
        return NoContent();
    }
}

public record RegisterToolRequest(
    string Code,
    string Name,
    ToolType ToolType,
    string? Brand = null,
    string? Model = null,
    string? Specification = null,
    int? MaxUsageCount = null,
    int? PmIntervalCount = null,
    bool RequiresCalibration = false,
    int? CalibrationIntervalDays = null,
    string? StorageLocation = null,
    DateOnly? PurchaseDate = null,
    decimal? PurchaseCost = null,
    string? Notes = null);

public record UpdateToolRequest(
    string Name,
    ToolType ToolType,
    bool IsActive,
    string? Brand = null,
    string? Model = null,
    string? Specification = null,
    int? MaxUsageCount = null,
    int? PmIntervalCount = null,
    bool RequiresCalibration = false,
    int? CalibrationIntervalDays = null,
    string? StorageLocation = null,
    DateOnly? PurchaseDate = null,
    decimal? PurchaseCost = null,
    string? Notes = null);

public record AddToolOperationRequest(
    string OperationCode,
    string? ProductCode = null,
    bool IsRequired = true,
    decimal UsageCountPerOp = 1m);

public record CheckoutToolRequest(int WorkCenterId, string? CheckedOutBy = null, DateTime? ExpectedReturnAt = null);
public record ReturnToolRequest(ToolReturnCondition Condition, string? ReturnedBy = null, string? Notes = null);
public record SendToolServiceRequest(ToolServiceType ServiceType);

public record RecordToolMaintenanceRequest(
    ToolMaintenanceType MaintenanceType,
    DateTime PerformedAt,
    string? PerformedBy = null,
    decimal? Cost = null,
    int? NextDueCount = null,
    DateOnly? NextDueDate = null,
    string? Notes = null);

public record RecordToolUsageRequest(int Count);

public record ToolCreatedResult(string ToolCode);
public record ToolOperationAddedResult(int MappingId);
public record ToolCheckedOutResult(long CheckoutId);
public record ToolMaintenanceLoggedResult(long LogId);
