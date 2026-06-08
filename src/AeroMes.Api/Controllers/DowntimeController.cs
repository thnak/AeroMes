using AeroMes.Application.Common;
using AeroMes.Application.Downtime.Commands.EndDowntime;
using AeroMes.Application.Downtime.Commands.StartDowntime;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/downtime")]
[Authorize]
public class DowntimeController(IMediator mediator) : ControllerBase
{
    [HttpPost("start")]
    public async Task<ActionResult<ApiResponse<object>>> Start(
        [FromBody] StartDowntimeRequest request,
        CancellationToken ct)
    {
        var id = await mediator.Send(new StartDowntimeCommand(
            request.MachineCode,
            request.ReasonCode,
            request.ReasonName,
            request.StartTime,
            request.OperatorId,
            request.Notes), ct);

        return StatusCode(201, new ApiResponse<object>(true, "Downtime started.",
            new { DowntimeLogId = id, Status = "DOWNTIME_ACTIVE" }));
    }

    [HttpPost("{downtimeLogId:long}/end")]
    public async Task<ActionResult<ApiResponse<object>>> End(
        long downtimeLogId,
        [FromBody] EndDowntimeRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new EndDowntimeCommand(downtimeLogId, request.EndTime, request.Notes), ct);

        return Ok(new ApiResponse<object>(true, "Downtime resolved.",
            new { result.DowntimeLogId, result.DurationMinutes, Status = "RESOLVED" }));
    }
}

public record StartDowntimeRequest(
    string MachineCode,
    string ReasonCode,
    string? ReasonName,
    DateTime StartTime,
    string OperatorId,
    string? Notes = null);

public record EndDowntimeRequest(DateTime EndTime, string? Notes = null);
