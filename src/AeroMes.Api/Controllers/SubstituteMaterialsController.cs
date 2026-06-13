using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Master.SubstituteMaterials.Commands.CreateSubstituteMaterial;
using AeroMes.Application.Master.SubstituteMaterials.Commands.DeleteSubstituteMaterial;
using AeroMes.Application.Master.SubstituteMaterials.Commands.SetSubstituteMaterialStatus;
using AeroMes.Application.Master.SubstituteMaterials.Commands.UpdateSubstituteMaterial;
using AeroMes.Application.Master.SubstituteMaterials.Queries.GetSubstituteMaterialById;
using AeroMes.Application.Master.SubstituteMaterials.Queries.GetSubstituteMaterials;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/substitute-materials")]
[Authorize]
public class SubstituteMaterialsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<SubstituteMaterialDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? primaryMaterialCode,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        bool? activeOnly = status is null ? null : string.Equals(status, "active", StringComparison.OrdinalIgnoreCase);
        var result = await queryMediator.QueryAsync(
            new GetSubstituteMaterialsQuery(primaryMaterialCode, activeOnly), null, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<SubstituteMaterialDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetSubstituteMaterialByIdQuery(id), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("by-material/{materialCode}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<SubstituteMaterialDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByMaterial(string materialCode, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetSubstituteMaterialsQuery(materialCode, ActiveOnly: true), null, ct);
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<SubstituteMaterialCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateSubstituteMaterialRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateSubstituteMaterialCommand(
                req.PrimaryMaterialCode,
                req.SubstituteMaterialCode,
                req.ConversionRatio,
                req.Priority,
                req.Notes,
                req.EffectiveDate,
                req.ExpiryDate,
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.SubstituteId }, result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSubstituteMaterialRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateSubstituteMaterialCommand(
                id,
                req.ConversionRatio,
                req.Priority,
                req.Notes,
                req.EffectiveDate,
                req.ExpiryDate,
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SetStatus(int id, [FromBody] SetSubstituteMaterialStatusRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SetSubstituteMaterialStatusCommand(id, req.IsActive, User.Identity?.Name), null, ct);
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
            new DeleteSubstituteMaterialCommand(id, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateSubstituteMaterialRequest(
    string PrimaryMaterialCode,
    string SubstituteMaterialCode,
    decimal ConversionRatio,
    int Priority = 0,
    string? Notes = null,
    DateOnly? EffectiveDate = null,
    DateOnly? ExpiryDate = null);

public record UpdateSubstituteMaterialRequest(
    decimal ConversionRatio,
    int Priority,
    string? Notes,
    DateOnly? EffectiveDate,
    DateOnly? ExpiryDate);

public record SetSubstituteMaterialStatusRequest(bool IsActive);
