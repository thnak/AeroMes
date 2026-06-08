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
    [HttpPost("submit-output")]
    public async Task<ActionResult<ApiResponse<SubmitOutputResult>>> SubmitOutput(
        [FromHeader(Name = "X-Idempotency-Key")] string? idempotencyKey,
        [FromBody] SubmitOutputRequest request,
        CancellationToken ct)
    {
        var cmd = new SubmitOutputCommand(
            request.WorkOrderId,
            request.OperatorId,
            request.MachineCode,
            request.ShiftCode,
            request.QtyOk,
            request.QtyNg,
            request.Timestamp,
            request.Defects.Select(d => new DefectEntry(d.DefectCode, d.Qty)).ToList(),
            idempotencyKey);

        var result = await mediator.Send(cmd, ct);
        return StatusCode(201, new ApiResponse<SubmitOutputResult>(true,
            "Production output recorded successfully.", result));
    }

    [HttpGet("oee")]
    public async Task<ActionResult<ApiResponse<OeeResult>>> GetOee(
        [FromQuery] int workCenterId,
        [FromQuery] string machineCode,
        [FromQuery] DateTime shiftStart,
        [FromQuery] DateTime shiftEnd,
        [FromQuery] double cycleTimeSeconds,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetOeeQuery(workCenterId, machineCode, shiftStart, shiftEnd, cycleTimeSeconds), ct);
        return Ok(new ApiResponse<OeeResult>(true, "OK", result));
    }
}

public record SubmitOutputRequest(
    int WorkOrderId,
    string OperatorId,
    string MachineCode,
    string ShiftCode,
    int QtyOk,
    int QtyNg,
    DateTime Timestamp,
    List<SubmitDefectEntry> Defects
);

public record SubmitDefectEntry(string DefectCode, int Qty);
