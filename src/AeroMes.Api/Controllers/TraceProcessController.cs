using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Traceability.Commands.CloseProcessRecord;
using AeroMes.Application.Traceability.Commands.OpenProcessRecord;
using AeroMes.Application.Traceability.Commands.RecordProcessParameter;
using AeroMes.Application.Traceability.Queries.GetLotAsBuiltRecord;
using AeroMes.Application.Traceability.Queries.GetMidSessionWIP;
using AeroMes.Application.Traceability.Queries.GetParametersForStep;
using AeroMes.Application.Traceability.Queries.GetProcessRecord;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/trace")]
[Authorize]
public class TraceProcessController(
    IQueryMediator queryMediator,
    ICommandMediator commandMediator) : ControllerBase
{
    [HttpPost("process-records/open")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> OpenProcessRecord(
        [FromBody] OpenProcessRecordRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new OpenProcessRecordCommand(
                req.LotNumber, req.ProductCode, req.WorkOrderID, req.JobID,
                req.RoutingStepID, req.StepSequence, req.StepName, req.OperatorCode,
                req.MachineCode, req.BOMRevision, req.RoutingRevision,
                req.ControlPlanRev, req.CertificationRef, req.CalibrationRef), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created, result.Value!);
    }

    [HttpPost("process-records/{id:guid}/parameters")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RecordParameter(
        Guid id, [FromBody] RecordParameterRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RecordProcessParameterCommand(
                id, req.ParameterName, req.ActualValue, req.NominalValue,
                req.UoM, req.LSL, req.USL, req.DataSource), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("process-records/{id:guid}/close")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CloseProcessRecord(
        Guid id, [FromBody] CloseProcessRecordRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CloseProcessRecordCommand(id, req.Outcome, req.DeviationRef, req.OutputLotNumber), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpGet("process-records/{id:guid}")]
    [ProducesResponseType<ProcessRecordDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProcessRecord(Guid id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetProcessRecordQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("lots/{lotNumber}/as-built")]
    [ProducesResponseType<IReadOnlyList<ProcessRecordDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLotAsBuilt(string lotNumber, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetLotAsBuiltRecordQuery(lotNumber), null, ct);
        return Ok(result);
    }

    [HttpGet("wip/mid-session")]
    [ProducesResponseType<IReadOnlyList<ProcessRecordDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMidSessionWIP(
        [FromQuery] int? workOrderId,
        [FromQuery] string? machineCode,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetMidSessionWIPQuery(workOrderId, machineCode), null, ct);
        return Ok(result);
    }

    [HttpGet("process-records/{id:guid}/parameters")]
    [ProducesResponseType<IReadOnlyList<ProcessParameterDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetParameters(Guid id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetParametersForStepQuery(id), null, ct);
        return Ok(result);
    }
}

public record OpenProcessRecordRequest(
    string LotNumber,
    string ProductCode,
    int WorkOrderID,
    long? JobID,
    int RoutingStepID,
    int StepSequence,
    string StepName,
    string OperatorCode,
    string? MachineCode,
    string? BOMRevision,
    string? RoutingRevision,
    string? ControlPlanRev,
    string? CertificationRef,
    string? CalibrationRef);

public record RecordParameterRequest(
    string ParameterName,
    string ActualValue,
    string? NominalValue,
    string? UoM,
    string? LSL,
    string? USL,
    ParameterDataSource DataSource);

public record CloseProcessRecordRequest(
    StepOutcome Outcome,
    string? DeviationRef,
    string? OutputLotNumber);
