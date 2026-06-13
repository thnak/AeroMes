using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Downtime.Commands.EndDowntime;
using AeroMes.Application.Downtime.Commands.StartDowntime;
using AeroMes.Application.Downtime.Queries.GetDowntimeDetail;
using AeroMes.Application.Downtime.Queries.GetDowntimeLogs;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/downtime")]
[Authorize]
public class DowntimeController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.WorkOrderRead)]
    [ProducesResponseType<ApiResponse<IReadOnlyList<DowntimeLogDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DowntimeLogDto>>>> GetAll(
        [FromQuery] string? machineCode,
        [FromQuery] bool? isOpen,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetDowntimeLogsQuery(machineCode, isOpen, from, to), null, ct);
        return Ok(new ApiResponse<IReadOnlyList<DowntimeLogDto>>(true, "OK", result));
    }

    [HttpGet("{id:long}")]
    [RequirePermission(Permissions.WorkOrderRead)]
    [ProducesResponseType<ApiResponse<DowntimeLogDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<DowntimeLogDto>>> GetById(long id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetDowntimeDetailQuery(id), null, ct);
        return Ok(new ApiResponse<DowntimeLogDto>(true, "OK", result));
    }

    [HttpPost("start")]
    [RequirePermission(Permissions.DowntimeDeclare)]
    [ProducesResponseType<ApiResponse<DowntimeStartedResult>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<DowntimeStartedResult>>> Start(
        [FromBody] StartDowntimeRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new StartDowntimeCommand(
            request.MachineCode,
            request.ReasonCode,
            request.ReasonName,
            request.StartTime,
            request.OperatorId,
            request.Notes), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();

        return StatusCode(StatusCodes.Status201Created,
            new ApiResponse<DowntimeStartedResult>(true, "Downtime started.", new DowntimeStartedResult(result.Value!)));
    }

    [HttpPost("{downtimeLogId:long}/end")]
    [RequirePermission(Permissions.DowntimeDeclare)]
    [ProducesResponseType<ApiResponse<EndDowntimeResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<EndDowntimeResult>>> End(
        long downtimeLogId,
        [FromBody] EndDowntimeRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new EndDowntimeCommand(downtimeLogId, request.EndTime, request.Notes), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();

        return Ok(new ApiResponse<EndDowntimeResult>(true, "Downtime resolved.", result.Value!));
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
public record DowntimeStartedResult(long DowntimeLogId, string Status = "DOWNTIME_ACTIVE");
