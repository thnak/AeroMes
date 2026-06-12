using AeroMes.Api.Auth;
using AeroMes.Application.Master.Boms.Commands.ActivateBomVersion;
using AeroMes.Application.Master.Boms.Commands.ApproveBom;
using AeroMes.Application.Master.Boms.Commands.CreateBomDraft;
using AeroMes.Application.Master.Boms.Commands.SubmitBomForReview;
using AeroMes.Application.Master.Boms.Commands.UpdateBomLines;
using AeroMes.Application.Master.Boms.Queries.CompareBomVersions;
using AeroMes.Application.Master.Boms.Queries.ExplodeBom;
using AeroMes.Application.Master.Boms.Queries.GetActiveBom;
using AeroMes.Application.Master.Boms.Queries.GetBomVersionDetail;
using AeroMes.Application.Master.Boms.Queries.GetBomVersions;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

/// <summary>
/// Versioned BOM with ECR/ECO change control. The legacy flat BOM remains
/// at /api/v1/bom-items until work-order resolution switches over (#12).
/// </summary>
[ApiController]
[Route("api/v1/bom")]
[Authorize]
public class BomController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet("{productCode}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<BomVersionDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActive(string productCode, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetActiveBomQuery(productCode), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{productCode}/explode")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<ExplodedBomLineDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Explode(
        string productCode, [FromQuery] decimal quantity = 1m, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new ExplodeBomQuery(productCode, quantity), null, ct));

    [HttpGet("{productCode}/compare")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<BomCompareDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Compare(
        string productCode, [FromQuery] string from, [FromQuery] string to, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new CompareBomVersionsQuery(productCode, from, to), null, ct));

    [HttpGet("{productCode}/versions")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<BomVersionDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVersions(string productCode, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetBomVersionsQuery(productCode), null, ct));

    [HttpGet("{productCode}/versions/{version}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<BomVersionDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVersion(string productCode, string version, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetBomVersionDetailQuery(productCode, version), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{productCode}/versions")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<BomDraftCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateDraft(
        string productCode, [FromBody] CreateBomDraftRequest req, CancellationToken ct)
    {
        var id = await commandMediator.SendAsync(
            new CreateBomDraftCommand(
                productCode, req.Version, req.BaseQuantity, req.Notes,
                req.CloneFromVersion, User.Identity?.Name), null, ct);
        return CreatedAtAction(nameof(GetVersion),
            new { productCode, version = req.Version }, new BomDraftCreatedResult(id, req.Version));
    }

    [HttpPut("{productCode}/versions/{version}/lines")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateLines(
        string productCode, string version, [FromBody] UpdateBomLinesRequest req, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new UpdateBomLinesCommand(productCode, version, req.Lines, User.Identity?.Name), null, ct);
        return NoContent();
    }

    [HttpPost("{productCode}/versions/{version}/submit")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Submit(string productCode, string version, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new SubmitBomForReviewCommand(productCode, version, User.Identity?.Name), null, ct);
        return NoContent();
    }

    [HttpPost("{productCode}/versions/{version}/approve")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Approve(string productCode, string version, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new ApproveBomCommand(productCode, version, User.Identity?.Name), null, ct);
        return NoContent();
    }

    [HttpPost("{productCode}/versions/{version}/activate")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Activate(
        string productCode, string version, [FromBody] ActivateBomRequest req, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new ActivateBomVersionCommand(productCode, version, req.EffectiveFrom, User.Identity?.Name), null, ct);
        return NoContent();
    }
}

public record CreateBomDraftRequest(
    string Version,
    decimal BaseQuantity = 1m,
    string? Notes = null,
    string? CloneFromVersion = null);

public record UpdateBomLinesRequest(IReadOnlyList<BomLineInput> Lines);
public record ActivateBomRequest(DateOnly? EffectiveFrom = null);
public record BomDraftCreatedResult(int BomHeaderId, string Version);
