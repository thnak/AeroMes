using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Cost.EnergyTariffs.Commands.CreateEnergyTariff;
using AeroMes.Application.Cost.EnergyTariffs.Commands.SetTariffStatus;
using AeroMes.Application.Cost.EnergyTariffs.Queries.GetEnergyTariffs;
using AeroMes.Application.Cost.ItemCosts.Commands.SetItemStandardCost;
using AeroMes.Application.Cost.ItemCosts.Queries.GetItemActiveCost;
using AeroMes.Application.Cost.ItemCosts.Queries.GetItemCostHistory;
using AeroMes.Application.Cost.LaborGrades.Commands.UpsertLaborGrade;
using AeroMes.Application.Cost.LaborGrades.Queries.GetLaborGrades;
using AeroMes.Application.Cost.MachineCostRates.Commands.UpsertMachineCostRate;
using AeroMes.Application.Cost.MachineCostRates.Queries.GetMachineCostRates;
using AeroMes.Application.Cost.MachineCostRates.Queries.GetMachineTotalRate;
using AeroMes.Application.Cost.MachineEnergyProfiles.Commands.UpsertMachineEnergyProfile;
using AeroMes.Application.Cost.MachineEnergyProfiles.Queries.GetMachineEnergyProfiles;
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
public class CostRateMasterController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    // ── Labor Grades ──────────────────────────────────────────────────────

    [HttpGet("labor-grades")]
    [RequirePermission(Permissions.CostRead)]
    [ProducesResponseType<IReadOnlyList<LaborGradeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLaborGrades(
        [FromQuery] string? keyword, [FromQuery] bool includeExpired = false, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetLaborGradesQuery(keyword, includeExpired), null, ct));

    [HttpPost("labor-grades")]
    [RequirePermission(Permissions.CostWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertLaborGrade(
        [FromBody] UpsertLaborGradeRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpsertLaborGradeCommand(
                request.GradeCode, request.GradeName, request.HourlyRate,
                request.OvertimeMultiplier, request.EffectiveFrom, request.Currency,
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    // ── Machine Cost Rates ────────────────────────────────────────────────

    [HttpGet("machines/{machineCode}/cost-rates")]
    [RequirePermission(Permissions.CostRead)]
    [ProducesResponseType<IReadOnlyList<MachineCostRateDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMachineCostRates(
        string machineCode, [FromQuery] bool includeExpired = false, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetMachineCostRatesQuery(machineCode, includeExpired), null, ct));

    [HttpGet("machines/{machineCode}/total-rate")]
    [RequirePermission(Permissions.CostRead)]
    [ProducesResponseType<MachineTotalRateDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMachineTotalRate(string machineCode, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetMachineTotalRateQuery(machineCode), null, ct));

    [HttpPost("machines/{machineCode}/cost-rates")]
    [RequirePermission(Permissions.CostWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertMachineCostRate(
        string machineCode, [FromBody] UpsertMachineCostRateRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpsertMachineCostRateCommand(
                machineCode, request.RateType, request.RatePerHour,
                request.EffectiveFrom, request.Notes, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    // ── Energy Tariffs ────────────────────────────────────────────────────

    [HttpGet("energy-tariffs")]
    [RequirePermission(Permissions.CostRead)]
    [ProducesResponseType<IReadOnlyList<EnergyTariffDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnergyTariffs(
        [FromQuery] bool includeInactive = false, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetEnergyTariffsQuery(includeInactive), null, ct));

    [HttpPost("energy-tariffs")]
    [RequirePermission(Permissions.CostWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEnergyTariff(
        [FromBody] CreateEnergyTariffRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateEnergyTariffCommand(
                request.TariffName, request.TariffType, request.PeakRateKWh,
                request.OffPeakRateKWh, request.PeakHourStart, request.PeakHourEnd,
                request.EffectiveFrom), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpPatch("energy-tariffs/{id:int}/status")]
    [RequirePermission(Permissions.CostWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetTariffStatus(
        int id, [FromBody] SetTariffStatusRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SetTariffStatusCommand(id, request.Activate), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    // ── Machine Energy Profiles ────────────────────────────────────────────

    [HttpGet("machines/{machineCode}/energy-profiles")]
    [RequirePermission(Permissions.CostRead)]
    [ProducesResponseType<IReadOnlyList<MachineEnergyProfileDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMachineEnergyProfiles(
        string machineCode, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetMachineEnergyProfilesQuery(machineCode), null, ct));

    [HttpPost("machines/{machineCode}/energy-profiles")]
    [RequirePermission(Permissions.CostWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertMachineEnergyProfile(
        string machineCode, [FromBody] UpsertMachineEnergyProfileRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpsertMachineEnergyProfileCommand(
                machineCode, request.NominalKW, request.LoadFactor,
                request.TariffID, request.EffectiveFrom), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    // ── Item Cost History ─────────────────────────────────────────────────

    [HttpGet("products/{productCode}/active-cost")]
    [RequirePermission(Permissions.CostRead)]
    [ProducesResponseType<ItemCostHistoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetItemActiveCost(
        string productCode, [FromQuery] ItemCostType costType = ItemCostType.STANDARD, CancellationToken ct = default)
    {
        var result = await queryMediator.QueryAsync(new GetItemActiveCostQuery(productCode, costType), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("products/{productCode}/cost-history")]
    [RequirePermission(Permissions.CostRead)]
    [ProducesResponseType<IReadOnlyList<ItemCostHistoryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetItemCostHistory(
        string productCode, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetItemCostHistoryQuery(productCode), null, ct));

    [HttpPost("products/{productCode}/standard-cost")]
    [RequirePermission(Permissions.CostWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetItemStandardCost(
        string productCode, [FromBody] SetItemStandardCostRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SetItemStandardCostCommand(
                productCode, request.CostType, request.UnitCost,
                request.CostUoM, request.EffectiveFrom,
                request.SourceRef, request.ApprovedBy, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }
}

public record UpsertLaborGradeRequest(
    string GradeCode, string GradeName, decimal HourlyRate,
    decimal OvertimeMultiplier, DateOnly EffectiveFrom, string Currency = "VND");

public record UpsertMachineCostRateRequest(
    MachineCostRateType RateType, decimal RatePerHour, DateOnly EffectiveFrom, string? Notes);

public record CreateEnergyTariffRequest(
    string TariffName, EnergyTariffType TariffType,
    decimal PeakRateKWh, decimal? OffPeakRateKWh,
    TimeOnly? PeakHourStart, TimeOnly? PeakHourEnd, DateOnly EffectiveFrom);

public record SetTariffStatusRequest(bool Activate);

public record UpsertMachineEnergyProfileRequest(
    decimal NominalKW, decimal LoadFactor, int TariffID, DateOnly EffectiveFrom);

public record SetItemStandardCostRequest(
    ItemCostType CostType, decimal UnitCost, string CostUoM,
    DateOnly EffectiveFrom, string? SourceRef, string? ApprovedBy);
