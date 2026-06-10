using AeroMes.Api.Auth;
using AeroMes.Application.Common;
using AeroMes.Application.Jobs.Commands.FinishJob;
using AeroMes.Application.Jobs.Commands.StartJob;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/jobs")]
[Authorize]
public class JobsController(IMediator mediator) : ControllerBase
{
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
        var result = await mediator.Send(new StartJobCommand(
            request.WorkOrderId,
            request.MachineCode,
            request.ShiftCode,
            request.OperatorId,
            request.StartTime), ct);

        return StatusCode(StatusCodes.Status201Created, new ApiResponse<StartJobResult>(true, "Job started.", result));
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
        var result = await mediator.Send(new FinishJobCommand(jobId, request.EndTime), ct);
        return Ok(new ApiResponse<FinishJobResult>(true, "Job finished.", result));
    }
}

public record StartJobRequest(
    int WorkOrderId,
    string MachineCode,
    string ShiftCode,
    string OperatorId,
    DateTime? StartTime = null);

public record FinishJobRequest(DateTime? EndTime = null);
