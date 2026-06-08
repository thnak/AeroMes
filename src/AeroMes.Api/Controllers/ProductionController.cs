using AeroMes.Application.Common;
using AeroMes.Application.Production.Commands.SubmitOutput;
using AeroMes.Application.Production.Queries.GetOee;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/production")]
[Authorize]
public class ProductionController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Submit output (OK + NG) for an active Job. Idempotent via X-Idempotency-Key header.
    /// </summary>
    [HttpPost("submit-output")]
    public async Task<ActionResult<ApiResponse<SubmitOutputResult>>> SubmitOutput(
        [FromHeader(Name = "X-Idempotency-Key")] string? idempotencyKey,
        [FromBody] SubmitOutputRequest request,
        CancellationToken ct)
    {
        var cmd = new SubmitOutputCommand(
            request.JobId,
            request.QtyOk,
            request.QtyNg,
            request.DeviceIp,
            request.Notes,
            idempotencyKey,
            request.Timestamp,
            request.Defects.Select(d => new DefectEntry(d.DefectCode, d.Qty)).ToList());

        var result = await mediator.Send(cmd, ct);
        return StatusCode(201, new ApiResponse<SubmitOutputResult>(true,
            "Production output recorded successfully.", result));
    }

    /// <summary>
    /// Get OEE metrics for a machine within a shift window.
    /// </summary>
    [HttpGet("oee")]
    public async Task<ActionResult<ApiResponse<OeeResult>>> GetOee(
        [FromQuery] string machineCode,
        [FromQuery] DateTime shiftStart,
        [FromQuery] DateTime shiftEnd,
        [FromQuery] double cycleTimeSeconds,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetOeeQuery(machineCode, shiftStart, shiftEnd, cycleTimeSeconds), ct);
        return Ok(new ApiResponse<OeeResult>(true, "OK", result));
    }
}

public record SubmitOutputRequest(
    long JobId,
    int QtyOk,
    int QtyNg,
    string? DeviceIp,
    string? Notes,
    DateTime? Timestamp,
    List<SubmitDefectEntry> Defects);

public record SubmitDefectEntry(string DefectCode, int Qty);
