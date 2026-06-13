using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.CreateFinishedProductIntakeRequest;
using AeroMes.Application.Wms.Commands.DeleteFinishedProductIntakeRequest;
using AeroMes.Application.Wms.Commands.ReceiveFinishedProductIntakeRequest;
using AeroMes.Application.Wms.Commands.RecallFinishedProductIntakeRequest;
using AeroMes.Application.Wms.Commands.SendFinishedProductIntakeRequest;
using AeroMes.Application.Wms.Commands.UpdateFinishedProductIntakeRequest;
using AeroMes.Application.Wms.Queries.GetFinishedProductIntakeRequestById;
using AeroMes.Application.Wms.Queries.GetFinishedProductIntakeRequests;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/finished-product-intake-requests")]
[Authorize]
public class FinishedProductIntakeRequestsController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.FinishedProductIntakeRead)]
    [ProducesResponseType<IReadOnlyList<FinishedProductIntakeRequestSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] IntakeRequestPurpose? intakePurpose,
        [FromQuery] IntakeRequestStatus? status,
        [FromQuery] int? productionOrderId,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetFinishedProductIntakeRequestsQuery(intakePurpose, status, productionOrderId), null, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.FinishedProductIntakeRead)]
    [ProducesResponseType<FinishedProductIntakeRequestDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetFinishedProductIntakeRequestByIdQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.FinishedProductIntakeCreate)]
    [ProducesResponseType<FinishedProductIntakeRequestCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateFinishedProductIntakeRequestRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new CreateFinishedProductIntakeRequestCommand(
            request.IntakePurpose,
            request.WarehouseType,
            request.ProductionOrderId,
            request.RequesterUnit,
            request.RequestDate,
            request.Notes,
            request.Lines,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.IntakeRequestId }, result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.FinishedProductIntakeUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateFinishedProductIntakeRequestRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new UpdateFinishedProductIntakeRequestCommand(
            id,
            request.IntakePurpose,
            request.WarehouseType,
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
    [RequirePermission(Permissions.FinishedProductIntakeDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteFinishedProductIntakeRequestCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/send")]
    [RequirePermission(Permissions.FinishedProductIntakeSend)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Send(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SendFinishedProductIntakeRequestCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/recall")]
    [RequirePermission(Permissions.FinishedProductIntakeRecall)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Recall(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RecallFinishedProductIntakeRequestCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/receive")]
    [RequirePermission(Permissions.FinishedProductIntakeReceive)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Receive(
        int id,
        [FromBody] ReceiveFinishedProductIntakeRequestRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new ReceiveFinishedProductIntakeRequestCommand(
            id,
            request.ReceiptLines,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateFinishedProductIntakeRequestRequest(
    IntakeRequestPurpose IntakePurpose,
    IntakeWarehouseType WarehouseType,
    int? ProductionOrderId,
    string RequesterUnit,
    DateTime RequestDate,
    string? Notes,
    IReadOnlyList<IntakeLineInput> Lines);

public record UpdateFinishedProductIntakeRequestRequest(
    IntakeRequestPurpose IntakePurpose,
    IntakeWarehouseType WarehouseType,
    int? ProductionOrderId,
    string RequesterUnit,
    DateTime RequestDate,
    string? Notes,
    IReadOnlyList<IntakeLineInput> Lines);

public record ReceiveFinishedProductIntakeRequestRequest(
    IReadOnlyList<ActualReceiptLineInput> ReceiptLines);
