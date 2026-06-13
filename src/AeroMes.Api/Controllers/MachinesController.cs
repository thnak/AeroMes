using AeroMes.Application.Master.Machines.Commands.ActivateMachine;
using AeroMes.Application.Master.Machines.Commands.CreateMachine;
using AeroMes.Application.Master.Machines.Commands.DeactivateMachine;
using AeroMes.Application.Master.Machines.Commands.DeleteMachine;
using AeroMes.Application.Master.Machines.Commands.DuplicateMachine;
using AeroMes.Application.Master.Machines.Commands.UpdateMachine;
using AeroMes.Application.Master.Machines.Commands.UpdateMachineCapacity;
using AeroMes.Application.Master.Machines.Queries.GetMachines;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/machines")]
[Authorize]
public class MachinesController(ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<MachineDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetMachinesQuery(activeOnly), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<MachineCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateMachineRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateMachineCommand(req.Code, req.Name, req.WorkCenterId, req.Brand, req.Model, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetAll), null, new MachineCreatedResult(result.Value!));
    }

    [HttpPut("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(string code, [FromBody] UpdateMachineRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateMachineCommand(code, req.Name, req.WorkCenterId, req.Brand, req.Model, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{code}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(string code, CancellationToken ct)
    {
        await commandMediator.SendAsync(new DeleteMachineCommand(code, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }

    [HttpGet("{code}/oee-target")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<MachineDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOeeTarget(string code, CancellationToken ct)
    {
        var all = await queryMediator.QueryAsync(new GetMachinesQuery(false), null, ct);
        var machine = all.FirstOrDefault(x => x.MachineCode == code.ToUpperInvariant());
        if (machine is null) return NotFound();
        return Ok(machine);
    }

    [HttpPut("{code}/oee-target")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateOeeTarget(string code, [FromBody] UpdateMachineCapacityRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateMachineCapacityCommand(
                code,
                req.MachineCategory,
                req.TargetOeePct,
                req.TheoreticalCapacityPerHour,
                req.PlannedDowntimeMinPerShift,
                req.HourlyCostRate,
                req.OpcUaNodeId,
                req.RequiresCertification,
                req.CertificationCode,
                req.MaxOperators,
                User.Identity?.Name ?? "system"),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("{code}/activate")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Activate(string code, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ActivateMachineCommand(code, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("{code}/deactivate")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Deactivate(string code, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeactivateMachineCommand(code, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{code}/duplicate")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<MachineCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Duplicate(string code, [FromBody] DuplicateMachineRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DuplicateMachineCommand(code, req.NewCode, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetAll), null, new MachineCreatedResult(result.Value!));
    }
}

public record CreateMachineRequest(string Code, string Name, int WorkCenterId, string? Brand, string? Model);
public record DuplicateMachineRequest(string NewCode);
public record UpdateMachineRequest(string Name, int WorkCenterId, string? Brand, string? Model);
public record MachineCreatedResult(string MachineCode);
public record UpdateMachineCapacityRequest(
    string? MachineCategory,
    decimal? TargetOeePct,
    decimal? TheoreticalCapacityPerHour,
    int PlannedDowntimeMinPerShift,
    decimal? HourlyCostRate,
    string? OpcUaNodeId,
    bool RequiresCertification,
    string? CertificationCode,
    byte MaxOperators);
