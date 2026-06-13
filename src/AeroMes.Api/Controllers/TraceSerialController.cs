using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Traceability.Commands.CommissionSerialUnits;
using AeroMes.Application.Traceability.Commands.PackSerials;
using AeroMes.Application.Traceability.Commands.RecallSerial;
using AeroMes.Application.Traceability.Commands.ShipSerial;
using AeroMes.Application.Traceability.Commands.UnpackSerials;
using AeroMes.Application.Traceability.Queries.GetAffectedSerialsFromLot;
using AeroMes.Application.Traceability.Queries.GetSerialComponentLots;
using AeroMes.Application.Traceability.Queries.GetSerialTimeline;
using AeroMes.Application.Traceability.Queries.GetSerialUnit;
using AeroMes.Application.Traceability.Queries.GetSerialsInLot;
using AeroMes.Application.Traceability.Queries.GetSSCCContents;
using AeroMes.Application.Traceability.Services;
using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/trace/serials")]
[Authorize]
public class TraceSerialController(
    IQueryMediator queryMediator,
    ICommandMediator commandMediator) : ControllerBase
{
    [HttpPost("commission")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CommissionSerialUnits(
        [FromBody] CommissionSerialUnitsRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CommissionSerialUnitsCommand(
                req.WorkOrderID, req.LotNumber, req.ProductCode, req.Quantity,
                req.SerialStrategy, req.ProductionDate, req.ExpiryDate, req.GTIN,
                req.ComponentLots ?? []), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created, result.Value!);
    }

    [HttpPost("pack")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PackSerials([FromBody] PackSerialsRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new PackSerialsCommand(req.SerialNumbers, req.CaseSSCC, req.PalletSSCC,
                User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value!);
    }

    [HttpPost("unpack")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UnpackSSCC([FromBody] UnpackSerialsRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UnpackSerialsCommand(req.SSCC, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value!);
    }

    [HttpPost("{sn}/ship")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ShipSerial(string sn, [FromBody] ShipSerialRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ShipSerialCommand(sn, req.ShipmentRef), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{sn}/recall")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RecallSerial(string sn, [FromBody] RecallSerialRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RecallSerialCommand(sn, req.RecallID), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpGet("{sn}")]
    [ProducesResponseType<SerialUnitDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSerialUnit(string sn, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetSerialUnitQuery(sn), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{sn}/timeline")]
    [ProducesResponseType<IReadOnlyList<SerialEventDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTimeline(string sn, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetSerialTimelineQuery(sn), null, ct);
        return Ok(result);
    }

    [HttpGet("{sn}/component-lots")]
    [ProducesResponseType<IReadOnlyList<SerialLotLineageDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComponentLots(string sn, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetSerialComponentLotsQuery(sn), null, ct);
        return Ok(result);
    }

    [HttpGet("by-lot/{lotNumber}")]
    [ProducesResponseType<PagedResult<SerialUnitDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByLot(
        string lotNumber,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await queryMediator.QueryAsync(
            new GetSerialsInLotQuery(lotNumber, page, pageSize), null, ct);
        return Ok(result);
    }

    [HttpGet("by-component-lot/{lotNumber}")]
    [ProducesResponseType<IReadOnlyList<SerialUnitDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAffectedByComponentLot(string lotNumber, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetAffectedSerialsFromLotQuery(lotNumber), null, ct);
        return Ok(result);
    }
}

[ApiController]
[Route("api/v1/trace/sscc")]
[Authorize]
public class TraceSSCCController(IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet("{sscc}/contents")]
    [ProducesResponseType<SSCCContentsDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSSCCContents(string sscc, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetSSCCContentsQuery(sscc), null, ct);
        return Ok(result);
    }
}

public record CommissionSerialUnitsRequest(
    int WorkOrderID,
    string LotNumber,
    string ProductCode,
    int Quantity,
    SerialStrategy SerialStrategy,
    DateOnly ProductionDate,
    DateOnly? ExpiryDate,
    string? GTIN,
    IReadOnlyList<LotConsumptionDto>? ComponentLots);

public record PackSerialsRequest(
    IReadOnlyList<string> SerialNumbers,
    string CaseSSCC,
    string? PalletSSCC);

public record UnpackSerialsRequest(string SSCC);

public record ShipSerialRequest(string ShipmentRef);

public record RecallSerialRequest(Guid RecallID);
