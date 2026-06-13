using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Master.EngChanges.Commands.ApproveEngChange;
using AeroMes.Application.Master.EngChanges.Commands.CreateEcoFromEcr;
using AeroMes.Application.Master.EngChanges.Commands.CreateEngChange;
using AeroMes.Application.Master.EngChanges.Commands.ImplementEco;
using AeroMes.Application.Master.EngChanges.Commands.RejectEngChange;
using AeroMes.Application.Master.EngChanges.Commands.SubmitEngChangeForReview;
using AeroMes.Application.Master.EngChanges.Queries.GetEngChangeByNumber;
using AeroMes.Application.Master.EngChanges.Queries.GetEngChanges;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/eng-changes")]
[Authorize]
public class EngChangesController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<EngChangeDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] EcStatus? status = null,
        [FromQuery] EcType? ecType = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetEngChangesQuery(status, ecType, search), null, ct));

    [HttpGet("{ecNumber}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<EngChangeDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByNumber(string ecNumber, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetEngChangeByNumberQuery(ecNumber), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<EngChangeCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateEngChangeRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateEngChangeCommand(
                req.EcNumber, req.EcType, req.Title, req.Description,
                req.Reason, req.Priority, req.TargetDate, req.AffectedProducts,
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        var ecNumber = result.Value!;
        return CreatedAtAction(nameof(GetByNumber), new { ecNumber }, new EngChangeCreatedResult(ecNumber));
    }

    [HttpPost("{ecNumber}/submit")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Submit(string ecNumber, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new SubmitEngChangeForReviewCommand(ecNumber, User.Identity?.Name), null, ct);
        return NoContent();
    }

    [HttpPost("{ecNumber}/approve")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Approve(string ecNumber, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new ApproveEngChangeCommand(ecNumber, User.Identity?.Name), null, ct);
        return NoContent();
    }

    [HttpPost("{ecNumber}/reject")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Reject(string ecNumber, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new RejectEngChangeCommand(ecNumber, User.Identity?.Name), null, ct);
        return NoContent();
    }

    [HttpPost("{ecNumber}/eco")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<EngChangeCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateEco(string ecNumber, [FromBody] CreateEcoRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateEcoFromEcrCommand(ecNumber, req.NewEcNumber, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        var ecoNumber = result.Value!;
        return CreatedAtAction(nameof(GetByNumber),
            new { ecNumber = ecoNumber }, new EngChangeCreatedResult(ecoNumber));
    }

    [HttpPost("{ecNumber}/implement")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<ImplementEcoResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Implement(string ecNumber, [FromBody] ImplementEcoRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ImplementEcoCommand(
                ecNumber, req.ProductCode, req.NewVersion, req.CloneFromActive,
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetByNumber), new { ecNumber }, result.Value!);
    }
}

public record CreateEngChangeRequest(
    string EcNumber,
    EcType EcType,
    string Title,
    EcReason Reason,
    string? Description = null,
    EcPriority Priority = EcPriority.Normal,
    DateOnly? TargetDate = null,
    string? AffectedProducts = null);

public record CreateEcoRequest(string NewEcNumber);
public record ImplementEcoRequest(string ProductCode, string NewVersion, bool CloneFromActive = true);
public record EngChangeCreatedResult(string EcNumber);
