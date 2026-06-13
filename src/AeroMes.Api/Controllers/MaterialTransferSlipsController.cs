using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.CreateMaterialTransferSlip;
using AeroMes.Application.Wms.Commands.DeleteMaterialTransferSlip;
using AeroMes.Application.Wms.Commands.UpdateMaterialTransferSlip;
using AeroMes.Application.Wms.Queries.GetMaterialTransferSlipById;
using AeroMes.Application.Wms.Queries.GetMaterialTransferSlips;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/material-transfer-slips")]
[Authorize]
public class MaterialTransferSlipsController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MaterialTransferRead)]
    [ProducesResponseType<IReadOnlyList<MaterialTransferSlipSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] MaterialTransferType? transferType,
        [FromQuery] MaterialTransferStatus? status,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetMaterialTransferSlipsQuery(transferType, status), null, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.MaterialTransferRead)]
    [ProducesResponseType<MaterialTransferSlipDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetMaterialTransferSlipByIdQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.MaterialTransferCreate)]
    [ProducesResponseType<MaterialTransferSlipCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMaterialTransferSlipRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new CreateMaterialTransferSlipCommand(
            request.TransferType,
            request.ReferenceRequestId,
            request.SourceWarehouseId,
            request.DestinationWarehouseId,
            request.Notes,
            request.Lines,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.SlipId }, result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MaterialTransferUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateMaterialTransferSlipRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new UpdateMaterialTransferSlipCommand(
            id,
            request.TransferType,
            request.ReferenceRequestId,
            request.SourceWarehouseId,
            request.DestinationWarehouseId,
            request.Notes,
            request.Lines,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MaterialTransferDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteMaterialTransferSlipCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateMaterialTransferSlipRequest(
    MaterialTransferType TransferType,
    int? ReferenceRequestId,
    int SourceWarehouseId,
    int DestinationWarehouseId,
    string? Notes,
    IReadOnlyList<TransferLineInput> Lines);

public record UpdateMaterialTransferSlipRequest(
    MaterialTransferType TransferType,
    int? ReferenceRequestId,
    int SourceWarehouseId,
    int DestinationWarehouseId,
    string? Notes,
    IReadOnlyList<TransferLineInput> Lines);
