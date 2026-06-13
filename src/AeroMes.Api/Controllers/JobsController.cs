using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Jobs.Commands.FinishJob;
using AeroMes.Application.Jobs.Commands.StartJob;
using AeroMes.Application.Jobs.Queries.GetJobDetail;
using AeroMes.Application.Jobs.Queries.GetJobs;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/jobs")]
[Authorize]
public class JobsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.WorkOrderRead)]
    [ProducesResponseType<ApiResponse<IReadOnlyList<JobDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<JobDto>>>> GetAll(
        [FromQuery] int? woId,
        [FromQuery] string? machineCode,
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetJobsQuery(woId, machineCode, status, from, to), null, ct);
        return Ok(new ApiResponse<IReadOnlyList<JobDto>>(true, "OK", result));
    }

    [HttpGet("{id:long}")]
    [RequirePermission(Permissions.WorkOrderRead)]
    [ProducesResponseType<ApiResponse<JobDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<JobDetailDto>>> GetById(long id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetJobDetailQuery(id), null, ct);
        if (!result.IsFound) return NotFound(result.ErrorMessage);
        return Ok(new ApiResponse<JobDetailDto>(true, "OK", result.Value!));
    }

    [HttpPost]
    [RequirePermission(Permissions.JobStart)]
    [ProducesResponseType<ApiResponse<StartJobResult>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<StartJobResult>>> Start(
        [FromBody] StartJobRequest request,
        CancellationToken ct)
    {
        var cmdResult = await commandMediator.SendAsync(new StartJobCommand(
            request.WorkOrderId,
            request.MachineCode,
            request.ShiftCode,
            request.OperatorId,
            request.StartTime), null, ct);
        if (!cmdResult.IsSuccess) return cmdResult.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created, new ApiResponse<StartJobResult>(true, "Job started.", cmdResult.Value!));
    }

    [HttpPost("{jobId:long}/finish")]
    [RequirePermission(Permissions.JobComplete)]
    [ProducesResponseType<ApiResponse<FinishJobResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<FinishJobResult>>> Finish(
        long jobId,
        [FromBody] FinishJobRequest request,
        CancellationToken ct)
    {
        var cmdResult = await commandMediator.SendAsync(new FinishJobCommand(jobId, request.EndTime), null, ct);
        if (!cmdResult.IsSuccess) return cmdResult.ToErrorResult();
        return Ok(new ApiResponse<FinishJobResult>(true, "Job finished.", cmdResult.Value!));
    }
}

public record StartJobRequest(
    int WorkOrderId,
    string MachineCode,
    string ShiftCode,
    string OperatorId,
    DateTime? StartTime = null);

public record FinishJobRequest(DateTime? EndTime = null);
