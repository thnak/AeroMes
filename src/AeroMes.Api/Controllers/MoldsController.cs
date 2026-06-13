using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Master.Molds.Commands.AddMoldProduct;
using AeroMes.Application.Master.Molds.Commands.AssignMoldToJob;
using AeroMes.Application.Master.Molds.Commands.AssignMoldToMachine;
using AeroMes.Application.Master.Molds.Commands.CompleteMoldMaintenance;
using AeroMes.Application.Master.Molds.Commands.DeleteMold;
using AeroMes.Application.Master.Molds.Commands.IncrementMoldShotCount;
using AeroMes.Application.Master.Molds.Commands.RecordMoldShots;
using AeroMes.Application.Master.Molds.Commands.RegisterMold;
using AeroMes.Application.Master.Molds.Commands.RemoveMoldProduct;
using AeroMes.Application.Master.Molds.Commands.ScrapMold;
using AeroMes.Application.Master.Molds.Commands.SendMoldForMaintenance;
using AeroMes.Application.Master.Molds.Commands.SetMoldCompatibility;
using AeroMes.Application.Master.Molds.Commands.UnmountMold;
using AeroMes.Application.Master.Molds.Commands.UpdateMold;
using AeroMes.Application.Master.Molds.Queries.GetCompatibleMoldsForMachine;
using AeroMes.Application.Master.Molds.Queries.GetMoldAssignmentHistory;
using AeroMes.Application.Master.Molds.Queries.GetMoldByCode;
using AeroMes.Application.Master.Molds.Queries.GetMolds;
using AeroMes.Application.Master.Molds.Queries.GetMoldsDueForPm;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/molds")]
[Authorize]
public class MoldsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<MoldDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool activeOnly = true,
        [FromQuery] MoldStatus? status = null,
        [FromQuery] string? machineCode = null,
        [FromQuery] string? productCode = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetMoldsQuery(activeOnly, status, machineCode, productCode, search), null, ct));

    [HttpGet("due-for-pm")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<MoldPmDueDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDueForPm(CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetMoldsDueForPmQuery(), null, ct));

    [HttpGet("{code}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<MoldDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetMoldByCodeQuery(code), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<MoldCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register([FromBody] RegisterMoldRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RegisterMoldCommand(
                req.Code, req.Name, req.MoldType, req.Material, req.Cavities,
                req.MaxShots, req.PmIntervalShots, req.Manufacturer,
                req.PurchaseDate, req.PurchaseCost, req.WeightKg,
                req.StorageLocation, req.Notes, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetByCode), new { code = result.Value! }, new MoldCreatedResult(result.Value!));
    }

    [HttpPut("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateMoldRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateMoldCommand(
                code, req.Name, req.MoldType, req.Material, req.Cavities,
                req.MaxShots, req.PmIntervalShots, req.Manufacturer,
                req.PurchaseDate, req.PurchaseCost, req.WeightKg,
                req.StorageLocation, req.Notes, req.IsActive, User.Identity?.Name), null, ct);
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
        var result = await commandMediator.SendAsync(
            new DeleteMoldCommand(code, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // ── Product mappings ────────────────────────────────────────────────────

    [HttpPost("{code}/products")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<MoldProductAddedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddProduct(string code, [FromBody] AddMoldProductRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AddMoldProductCommand(code, req.ProductCode, req.IsDefault, req.CycleTimeSeconds, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetByCode), new { code }, new MoldProductAddedResult(result.Value!));
    }

    [HttpDelete("{code}/products/{mappingId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveProduct(string code, int mappingId, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RemoveMoldProductCommand(code, mappingId, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // ── Machine assignment ──────────────────────────────────────────────────

    [HttpPost("{code}/assign")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Assign(string code, [FromBody] AssignMoldRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AssignMoldToMachineCommand(code, req.MachineCode, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{code}/unassign")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Unassign(string code, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new UnmountMoldCommand(code, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // ── Maintenance & shots ─────────────────────────────────────────────────

    [HttpPost("{code}/maintenance/send")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SendForMaintenance(string code, [FromBody] SendMoldMaintenanceRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SendMoldForMaintenanceCommand(code, req.MaintenanceType, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{code}/maintenance/complete")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<MoldMaintenanceLoggedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CompleteMaintenance(string code, [FromBody] CompleteMoldMaintenanceRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CompleteMoldMaintenanceCommand(
                code, req.MaintenanceType, req.StartDate, req.EndDate,
                req.TechnicianId, req.Description, req.PartReplaced,
                req.Cost, req.NextDueShots, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetByCode), new { code }, new MoldMaintenanceLoggedResult(result.Value!));
    }

    [HttpPost("{code}/shots")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<RecordMoldShotsResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RecordShots(string code, [FromBody] RecordMoldShotsRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RecordMoldShotsCommand(code, req.Shots, User.Identity?.Name), null, ct);
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
        var result = await commandMediator.SendAsync(new ScrapMoldCommand(code, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // ── Mold–Machine Compatibility ─────────────────────────────────────────

    [HttpPut("{code}/compatibility/{machineCode}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SetCompatibility(
        string code, string machineCode, [FromBody] SetCompatibilityRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SetMoldCompatibilityCommand(code, machineCode, req.IsCompatible, req.Notes), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpGet("{code}/compatible-machines")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<MoldCompatibilityDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompatibleMachines(string code, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetCompatibleMoldsForMachineQuery(code), null, ct));

    // ── Job Assignment & Shot Tracking ─────────────────────────────────────

    [HttpPost("{code}/assign-job")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AssignToJob(string code, [FromBody] AssignMoldToJobRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AssignMoldToJobCommand(code, req.MachineCode, req.WOID, req.JobID,
                User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{code}/increment-shots")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> IncrementShots(string code, [FromBody] IncrementShotsRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new IncrementMoldShotCountCommand(code, req.JobID, req.QtyOK), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpGet("{code}/history")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<MoldAssignmentDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignmentHistory(
        string code,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetMoldAssignmentHistoryQuery(code, from, to), null, ct));
}

public record RegisterMoldRequest(
    string Code,
    string Name,
    MoldType MoldType,
    string? Material,
    long MaxShots,
    int PmIntervalShots,
    int Cavities = 1,
    string? Manufacturer = null,
    DateOnly? PurchaseDate = null,
    decimal? PurchaseCost = null,
    decimal? WeightKg = null,
    string? StorageLocation = null,
    string? Notes = null);

public record UpdateMoldRequest(
    string Name,
    MoldType MoldType,
    string? Material,
    long MaxShots,
    int PmIntervalShots,
    int Cavities,
    bool IsActive,
    string? Manufacturer = null,
    DateOnly? PurchaseDate = null,
    decimal? PurchaseCost = null,
    decimal? WeightKg = null,
    string? StorageLocation = null,
    string? Notes = null);

public record AddMoldProductRequest(string ProductCode, bool IsDefault = false, double? CycleTimeSeconds = null);
public record AssignMoldRequest(string MachineCode);
public record SendMoldMaintenanceRequest(MoldMaintenanceType MaintenanceType);

public record CompleteMoldMaintenanceRequest(
    MoldMaintenanceType MaintenanceType,
    DateTime StartDate,
    DateTime? EndDate = null,
    string? TechnicianId = null,
    string? Description = null,
    string? PartReplaced = null,
    decimal? Cost = null,
    long? NextDueShots = null);

public record RecordMoldShotsRequest(long Shots);

public record MoldCreatedResult(string MoldCode);
public record MoldProductAddedResult(int MappingId);
public record MoldMaintenanceLoggedResult(long LogId);
public record SetCompatibilityRequest(bool IsCompatible, string? Notes = null);
public record AssignMoldToJobRequest(string MachineCode, int WOID, long JobID);
public record IncrementShotsRequest(long JobID, long QtyOK);
