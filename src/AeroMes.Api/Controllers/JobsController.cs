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
    /// <summary>
    /// An operator starts a Job on a machine for a running Work Order.
    /// </summary>
    [HttpPost]
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

        return StatusCode(201, new ApiResponse<StartJobResult>(true, "Job started.", result));
    }

    /// <summary>
    /// Close a Job when the operator finishes their shift or the WO step is done.
    /// </summary>
    [HttpPost("{jobId:long}/finish")]
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
