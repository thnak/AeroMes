using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.ApproveMaterialSupplyRequest;
using AeroMes.Application.Wms.Commands.CreateMaterialSupplyRequest;
using AeroMes.Application.Wms.Commands.DeleteMaterialSupplyRequest;
using AeroMes.Application.Wms.Commands.RejectMaterialSupplyRequest;
using AeroMes.Application.Wms.Commands.SubmitMaterialSupplyRequest;
using AeroMes.Application.Wms.Commands.UpdateMaterialSupplyRequest;
using AeroMes.Application.Wms.Queries.GetMaterialSupplyRequestById;
using AeroMes.Application.Wms.Queries.GetMaterialSupplyRequests;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/material-supply-requests")]
[Authorize]
public class MaterialSupplyRequestsController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MaterialSupplyRequestRead)]
    [ProducesResponseType<IReadOnlyList<MaterialSupplyRequestSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] MaterialSupplyRequestType? requestType,
        [FromQuery] MaterialSupplyRequestStatus? status,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetMaterialSupplyRequestsQuery(requestType, status), null, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.MaterialSupplyRequestRead)]
    [ProducesResponseType<MaterialSupplyRequestDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetMaterialSupplyRequestByIdQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.MaterialSupplyRequestCreate)]
    [ProducesResponseType<MaterialSupplyRequestCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMaterialSupplyRequestRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new CreateMaterialSupplyRequestCommand(
            request.RequestType,
            request.RequesterUnit,
            request.RequiredByDate,
            request.Notes,
            request.Lines,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.RequestId }, result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MaterialSupplyRequestUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateMaterialSupplyRequestRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new UpdateMaterialSupplyRequestCommand(
            id,
            request.RequestType,
            request.RequesterUnit,
            request.RequiredByDate,
            request.Notes,
            request.Lines,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MaterialSupplyRequestDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteMaterialSupplyRequestCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/submit")]
    [RequirePermission(Permissions.MaterialSupplyRequestSubmit)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Submit(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SubmitMaterialSupplyRequestCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/approve")]
    [RequirePermission(Permissions.MaterialSupplyRequestApprove)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ApproveMaterialSupplyRequestCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpPost("{id:int}/reject")]
    [RequirePermission(Permissions.MaterialSupplyRequestApprove)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Reject(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new RejectMaterialSupplyRequestCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateMaterialSupplyRequestRequest(
    MaterialSupplyRequestType RequestType,
    string RequesterUnit,
    DateTime? RequiredByDate,
    string? Notes,
    IReadOnlyList<SupplyRequestLineInput> Lines);

public record UpdateMaterialSupplyRequestRequest(
    MaterialSupplyRequestType RequestType,
    string RequesterUnit,
    DateTime? RequiredByDate,
    string? Notes,
    IReadOnlyList<SupplyRequestLineInput> Lines);
