using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.CreateFactoryWarehouseReceipt;
using AeroMes.Application.Wms.Commands.DeleteFactoryWarehouseReceipt;
using AeroMes.Application.Wms.Commands.UpdateFactoryWarehouseReceipt;
using AeroMes.Application.Wms.Queries.GetFactoryWarehouseReceiptById;
using AeroMes.Application.Wms.Queries.GetFactoryWarehouseReceipts;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/factory-warehouse-receipts")]
[Authorize]
public class FactoryWarehouseReceiptsController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.FactoryWarehouseReceiptRead)]
    [ProducesResponseType<IReadOnlyList<FactoryWarehouseReceiptSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] FactoryReceiptType? receiptType,
        [FromQuery] FactoryReceiptStatus? status,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetFactoryWarehouseReceiptsQuery(receiptType, status), null, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.FactoryWarehouseReceiptRead)]
    [ProducesResponseType<FactoryWarehouseReceiptDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetFactoryWarehouseReceiptByIdQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.FactoryWarehouseReceiptCreate)]
    [ProducesResponseType<FactoryWarehouseReceiptCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateFactoryWarehouseReceiptRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new CreateFactoryWarehouseReceiptCommand(
            request.ReceiptType,
            request.ReferenceRequestId,
            request.SupplierOrTransferringUnit,
            request.Notes,
            request.Lines,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.ReceiptId }, result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.FactoryWarehouseReceiptUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateFactoryWarehouseReceiptRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new UpdateFactoryWarehouseReceiptCommand(
            id,
            request.ReceiptType,
            request.ReferenceRequestId,
            request.SupplierOrTransferringUnit,
            request.Notes,
            request.Lines,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.FactoryWarehouseReceiptDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteFactoryWarehouseReceiptCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateFactoryWarehouseReceiptRequest(
    FactoryReceiptType ReceiptType,
    int? ReferenceRequestId,
    string SupplierOrTransferringUnit,
    string? Notes,
    IReadOnlyList<ReceiptLineInput> Lines);

public record UpdateFactoryWarehouseReceiptRequest(
    FactoryReceiptType ReceiptType,
    int? ReferenceRequestId,
    string SupplierOrTransferringUnit,
    string? Notes,
    IReadOnlyList<ReceiptLineInput> Lines);
