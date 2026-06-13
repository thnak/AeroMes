using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Traceability.Commands.ApplyRecallQuarantine;
using AeroMes.Application.Traceability.Commands.CloseRecall;
using AeroMes.Application.Traceability.Commands.IdentifyRecallScope;
using AeroMes.Application.Traceability.Commands.InitiateRecall;
using AeroMes.Application.Traceability.Queries.GenerateRecallAuditReport;
using AeroMes.Application.Traceability.Queries.GetRecall;
using AeroMes.Application.Traceability.Queries.GetRecallAuditLog;
using AeroMes.Application.Traceability.Queries.GetRecallScope;
using AeroMes.Application.Traceability.Queries.ListRecalls;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/recalls")]
[Authorize]
public class RecallsController(
    IQueryMediator queryMediator,
    ICommandMediator commandMediator) : ControllerBase
{
    [HttpPost]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> InitiateRecall(
        [FromBody] InitiateRecallRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new InitiateRecallCommand(
                req.Title, req.RecallType, req.AnchorLotNumber, req.AnchorDirection,
                User.Identity?.Name ?? "system", req.Description, req.RegulatoryRef), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created, result.Value!);
    }

    [HttpPost("{id:guid}/identify-scope")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType<RecallScopeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> IdentifyScope(Guid id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new IdentifyRecallScopeCommand(id, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value!);
    }

    [HttpPost("{id:guid}/apply-quarantine")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType<RecallQuarantineResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ApplyQuarantine(Guid id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ApplyRecallQuarantineCommand(id, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value!);
    }

    [HttpPost("{id:guid}/close")]
    [RequirePermission(Permissions.QualityApprove)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CloseRecall(
        Guid id, [FromBody] CloseRecallRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CloseRecallCommand(id, User.Identity?.Name ?? "system", req.ESignatureToken, req.ClosureNotes), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType<PagedResult<RecallSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListRecalls(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await queryMediator.QueryAsync(new ListRecallsQuery(status, page, pageSize), null, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<RecallDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecall(Guid id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetRecallQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id:guid}/scope")]
    [ProducesResponseType<RecallScopeDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetScope(Guid id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetRecallScopeQuery(id), null, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/audit-log")]
    [ProducesResponseType<IReadOnlyList<RecallAuditEntryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLog(Guid id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetRecallAuditLogQuery(id), null, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/report")]
    [ProducesResponseType<RecallAuditReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuditReport(Guid id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GenerateRecallAuditReportQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }
}

public record InitiateRecallRequest(
    string Title,
    RecallType RecallType,
    string AnchorLotNumber,
    AnchorDirection AnchorDirection,
    string? Description,
    string? RegulatoryRef);

public record CloseRecallRequest(
    string ESignatureToken,
    string ClosureNotes);
