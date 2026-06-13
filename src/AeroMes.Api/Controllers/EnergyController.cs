using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Energy.Commands.CloseShiftConsumption;
using AeroMes.Application.Energy.Commands.RegisterMeter;
using AeroMes.Application.Energy.Commands.RegisterMeterReading;
using AeroMes.Application.Energy.Queries.GetEnergyIntensityTrend;
using AeroMes.Application.Energy.Queries.GetShiftEnergyReport;
using AeroMes.Domain.Energy;
using AeroMes.Domain.Energy.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/energy")]
[Authorize]
public class EnergyController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpPost("meters")]
    [RequirePermission(Permissions.EnergyWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterMeter(
        [FromBody] RegisterMeterRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RegisterMeterCommand(
                request.MeterCode, request.MeterName, request.UtilityType,
                request.Unit, request.MachineCode, request.WorkCenterID,
                request.IsSubMeter, request.ParentMeterID, request.TariffID,
                request.OpcUaNodeId, User.Identity?.Name),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpPost("readings")]
    [RequirePermission(Permissions.EnergyWrite)]
    [ProducesResponseType<long>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterReading(
        [FromBody] RegisterReadingRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RegisterMeterReadingCommand(
                request.MeterID, request.ReadingType, request.ReadingValue,
                request.ReadingAt ?? DateTime.UtcNow, request.ShiftCode,
                request.WOID, User.Identity?.Name, request.Notes),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpPost("shift-close/{shiftCode}/{date}")]
    [RequirePermission(Permissions.EnergyWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseShift(
        string shiftCode, DateOnly date,
        [FromBody] CloseShiftRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CloseShiftConsumptionCommand(
                request.MeterID, shiftCode, date, request.EndReadingID, request.QtyProduced),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpGet("shift-report")]
    [RequirePermission(Permissions.EnergyRead)]
    [ProducesResponseType<IReadOnlyList<ShiftEnergyDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShiftReport(
        [FromQuery] string? machineCode,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(
            new GetShiftEnergyReportQuery(machineCode, from, to), null, ct));

    [HttpGet("intensity-trend/{machineCode}")]
    [RequirePermission(Permissions.EnergyRead)]
    [ProducesResponseType<IReadOnlyList<EnergyIntensityTrendDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIntensityTrend(
        string machineCode, [FromQuery] int months = 6, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetEnergyIntensityTrendQuery(machineCode, months), null, ct));
}

public record RegisterMeterRequest(
    string MeterCode, string MeterName, UtilityType UtilityType, string Unit,
    string? MachineCode, int? WorkCenterID, bool IsSubMeter = false,
    int? ParentMeterID = null, int? TariffID = null, string? OpcUaNodeId = null);

public record RegisterReadingRequest(
    int MeterID, ReadingType ReadingType, decimal ReadingValue,
    DateTime? ReadingAt, string? ShiftCode, int? WOID, string? Notes);

public record CloseShiftRequest(int MeterID, long EndReadingID, int? QtyProduced);
