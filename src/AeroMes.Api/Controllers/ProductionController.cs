using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Production.Commands.SubmitOutput;
using AeroMes.Application.Production.Queries.GetOee;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/production")]
[Authorize]
public class ProductionController(ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpPost("submit-output")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType<ApiResponse<SubmitOutputResult>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<SubmitOutputResult>>> SubmitOutput(
        [FromHeader(Name = "X-Idempotency-Key")] string idempotencyKey,
        [FromBody] SubmitOutputRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return BadRequest("X-Idempotency-Key header is required.");

        var cmd = new SubmitOutputCommand(
            request.JobId,
            request.QtyOk,
            request.QtyNg,
            request.DeviceIp,
            request.Notes,
            idempotencyKey,
            request.Timestamp,
            request.Defects.Select(d => new DefectEntry(d.DefectCode, d.Qty)).ToList());

        var cmdResult = await commandMediator.SendAsync(cmd, null, ct);
        if (!cmdResult.IsSuccess) return cmdResult.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created, new ApiResponse<SubmitOutputResult>(true,
            "Production output recorded successfully.", cmdResult.Value!));
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
        var result = await queryMediator.QueryAsync(
            new GetOeeQuery(machineCode, shiftStart, shiftEnd, cycleTimeSeconds), null, ct);
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
