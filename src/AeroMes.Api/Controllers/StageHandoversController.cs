using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Production.StageHandovers.Commands.ConfirmHandoverForm;
using AeroMes.Application.Production.StageHandovers.Commands.CreateHandoverForm;
using AeroMes.Application.Production.StageHandovers.Queries.GetHandoverFormDetail;
using AeroMes.Application.Production.StageHandovers.Queries.GetHandoverForms;
using AeroMes.Domain.Production;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/stage-handovers")]
[Authorize]
public class StageHandoversController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<IReadOnlyList<HandoverFormSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? workOrderId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetHandoverFormsQuery(workOrderId, from, to), null, ct));

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<HandoverFormDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await queryMediator.QueryAsync(new GetHandoverFormDetailQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType<HandoverFormCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateHandoverFormRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateHandoverFormCommand(
                req.FormType,
                req.FromWorkOrderId, req.FromRoutingStepId,
                req.ToWorkOrderId, req.ToRoutingStepId,
                req.HandoverDate, req.Notes,
                req.Lines.Select(l => new HandoverLineSpec(l.ProductCode, l.Qty, l.Unit, l.Notes)).ToList(),
                User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return StatusCode(StatusCodes.Status201Created, new HandoverFormCreatedResult(result.Value!));
    }

    [HttpPost("{id:int}/confirm")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Confirm(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ConfirmHandoverFormCommand(id, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateHandoverFormRequest(
    HandoverFormType FormType,
    int FromWorkOrderId,
    int FromRoutingStepId,
    int ToWorkOrderId,
    int ToRoutingStepId,
    DateTime HandoverDate,
    string? Notes,
    IReadOnlyList<HandoverLineRequest> Lines);

public record HandoverLineRequest(string ProductCode, decimal Qty, string Unit, string? Notes);
public record HandoverFormCreatedResult(int FormId);
