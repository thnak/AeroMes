using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.CreateMaterialRequisition;
using AeroMes.Application.Wms.Commands.DeleteMaterialRequisition;
using AeroMes.Application.Wms.Commands.FulfillMaterialRequisition;
using AeroMes.Application.Wms.Commands.RecallMaterialRequisition;
using AeroMes.Application.Wms.Commands.SendMaterialRequisition;
using AeroMes.Application.Wms.Commands.UpdateMaterialRequisition;
using AeroMes.Application.Wms.Queries.GetMaterialRequisitionById;
using AeroMes.Application.Wms.Queries.GetMaterialRequisitions;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/material-requisitions")]
[Authorize]
public class MaterialRequisitionsController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MaterialRequisitionRead)]
    [ProducesResponseType<IReadOnlyList<MaterialRequisitionSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? productionOrderId,
        [FromQuery] MaterialRequisitionStatus? status,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetMaterialRequisitionsQuery(productionOrderId, status), null, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.MaterialRequisitionRead)]
    [ProducesResponseType<MaterialRequisitionDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetMaterialRequisitionByIdQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.MaterialRequisitionCreate)]
    [ProducesResponseType<MaterialRequisitionCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMaterialRequisitionRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new CreateMaterialRequisitionCommand(
            request.ProductionOrderId,
            request.RequesterUnit,
            request.RequestDate,
            request.Notes,
            request.Lines,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.RequisitionId }, result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MaterialRequisitionUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateMaterialRequisitionRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new UpdateMaterialRequisitionCommand(
            id,
            request.ProductionOrderId,
            request.RequesterUnit,
            request.RequestDate,
            request.Notes,
            request.Lines,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MaterialRequisitionDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteMaterialRequisitionCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/send")]
    [RequirePermission(Permissions.MaterialRequisitionSend)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Send(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SendMaterialRequisitionCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/recall")]
    [RequirePermission(Permissions.MaterialRequisitionRecall)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Recall(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RecallMaterialRequisitionCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/fulfill")]
    [RequirePermission(Permissions.MaterialRequisitionFulfill)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Fulfill(
        int id,
        [FromBody] FulfillMaterialRequisitionRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new FulfillMaterialRequisitionCommand(
            id,
            request.IssuanceLines,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateMaterialRequisitionRequest(
    int? ProductionOrderId,
    string RequesterUnit,
    DateTime RequestDate,
    string? Notes,
    IReadOnlyList<RequisitionLineInput> Lines);

public record UpdateMaterialRequisitionRequest(
    int? ProductionOrderId,
    string RequesterUnit,
    DateTime RequestDate,
    string? Notes,
    IReadOnlyList<RequisitionLineInput> Lines);

public record FulfillMaterialRequisitionRequest(
    IReadOnlyList<IssuanceLineInput> IssuanceLines);
