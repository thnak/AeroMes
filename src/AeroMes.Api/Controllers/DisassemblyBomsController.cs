using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Master.DisassemblyBoms.Commands.CreateDisassemblyBom;
using AeroMes.Application.Master.DisassemblyBoms.Commands.DeleteDisassemblyBom;
using AeroMes.Application.Master.DisassemblyBoms.Commands.SetDisassemblyBomDefault;
using AeroMes.Application.Master.DisassemblyBoms.Commands.SetDisassemblyBomStatus;
using AeroMes.Application.Master.DisassemblyBoms.Commands.UpdateDisassemblyBom;
using AeroMes.Application.Master.DisassemblyBoms.Queries.GetDisassemblyBomById;
using AeroMes.Application.Master.DisassemblyBoms.Queries.GetDisassemblyBoms;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/disassembly-boms")]
[Authorize]
public class DisassemblyBomsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<DisassemblyBomSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? sourceProductCode,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetDisassemblyBomsQuery(sourceProductCode, status), null, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<DisassemblyBomDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetDisassemblyBomByIdQuery(id), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<DisassemblyBomCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateDisassemblyBomRequest req, CancellationToken ct)
    {
        var lines = req.Lines.Select(l => new DisassemblyBomLineInput(
            l.LineNo, l.ComponentCode, l.ComponentType, l.RecoveryRate, l.FixedQuantity, l.UoMCode, l.Notes)).ToList();
        var result = await commandMediator.SendAsync(
            new CreateDisassemblyBomCommand(
                req.BomName,
                req.SourceProductCode,
                req.BomType,
                req.LossRatio,
                req.EffectiveDate,
                req.ExpiryDate,
                lines,
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.DisassemblyBomId }, result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDisassemblyBomRequest req, CancellationToken ct)
    {
        var lines = req.Lines.Select(l => new DisassemblyBomLineInput(
            l.LineNo, l.ComponentCode, l.ComponentType, l.RecoveryRate, l.FixedQuantity, l.UoMCode, l.Notes)).ToList();
        var result = await commandMediator.SendAsync(
            new UpdateDisassemblyBomCommand(
                id,
                req.BomName,
                req.LossRatio,
                req.EffectiveDate,
                req.ExpiryDate,
                lines,
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SetStatus(int id, [FromBody] SetDisassemblyBomStatusRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SetDisassemblyBomStatusCommand(id, req.IsActive, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/set-default")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SetDefault(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SetDisassemblyBomDefaultCommand(id, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteDisassemblyBomCommand(id, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateDisassemblyBomRequest(
    string BomName,
    string SourceProductCode,
    DisassemblyBomType BomType,
    decimal LossRatio,
    DateOnly? EffectiveDate,
    DateOnly? ExpiryDate,
    IReadOnlyList<DisassemblyBomLineInputRequest> Lines);

public record UpdateDisassemblyBomRequest(
    string BomName,
    decimal LossRatio,
    DateOnly? EffectiveDate,
    DateOnly? ExpiryDate,
    IReadOnlyList<DisassemblyBomLineInputRequest> Lines);

public record DisassemblyBomLineInputRequest(
    int LineNo,
    string ComponentCode,
    DisassemblyComponentType ComponentType,
    decimal? RecoveryRate,
    decimal? FixedQuantity,
    string UoMCode,
    string? Notes = null);

public record SetDisassemblyBomStatusRequest(bool IsActive);
