using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Production.Commands.ApproveRegrindUsage;
using AeroMes.Application.Production.Commands.RecordMaterialBlend;
using AeroMes.Application.Production.Queries.GetBlendLogForJob;
using AeroMes.Application.Production.Queries.GetNonCompliantBlends;
using AeroMes.Application.Production.Queries.GetRegrindUsageSummary;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/material-blends")]
[Authorize]
public class MaterialBlendsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpPost]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType<long>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Record([FromBody] RecordBlendRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RecordMaterialBlendCommand(
                request.JobID,
                request.ResinProductCode,
                request.VirginLotNumber,
                request.VirginQtyKg,
                request.RegrindLotNumber,
                request.RegrindQtyKg),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpPost("{blendLogId:long}/approve")]
    [RequirePermission(Permissions.QualityApprove)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(long blendLogId, [FromBody] ApproveBlendRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ApproveRegrindUsageCommand(blendLogId, request.ApprovedBy, request.ApprovalNotes),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpGet("by-job/{jobId:long}")]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<IReadOnlyList<MaterialBlendLogDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByJob(long jobId, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetBlendLogForJobQuery(jobId), null, ct));

    [HttpGet("non-compliant")]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<PagedResult<MaterialBlendLogDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNonCompliant(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] bool? isApproved,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetNonCompliantBlendsQuery(fromDate, toDate, isApproved, page, pageSize), null, ct));

    [HttpGet("summary")]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<IReadOnlyList<RegrindUsageSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? resinProductCode,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetRegrindUsageSummaryQuery(fromDate, toDate, resinProductCode), null, ct));
}

public record RecordBlendRequest(
    long JobID,
    string ResinProductCode,
    string VirginLotNumber,
    decimal VirginQtyKg,
    string? RegrindLotNumber,
    decimal RegrindQtyKg);

public record ApproveBlendRequest(string ApprovedBy, string? ApprovalNotes = null);
