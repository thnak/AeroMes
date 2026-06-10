using AeroMes.Api.Auth;
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
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType<ApiResponse<SubmitOutputResult>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
        return StatusCode(StatusCodes.Status201Created, new ApiResponse<SubmitOutputResult>(true,
            "Production output recorded successfully.", result));
    }

    [HttpGet("oee")]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<ApiResponse<OeeResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
