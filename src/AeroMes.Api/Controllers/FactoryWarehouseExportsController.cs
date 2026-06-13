using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.CreateFactoryWarehouseExport;
using AeroMes.Application.Wms.Commands.DeleteFactoryWarehouseExport;
using AeroMes.Application.Wms.Commands.UpdateFactoryWarehouseExport;
using AeroMes.Application.Wms.Queries.GetFactoryWarehouseExportById;
using AeroMes.Application.Wms.Queries.GetFactoryWarehouseExports;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/factory-warehouse-exports")]
[Authorize]
public class FactoryWarehouseExportsController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.FactoryWarehouseExportRead)]
    [ProducesResponseType<IReadOnlyList<FactoryWarehouseExportSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] FactoryExportType? exportType,
        [FromQuery] FactoryExportStatus? status,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetFactoryWarehouseExportsQuery(exportType, status), null, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.FactoryWarehouseExportRead)]
    [ProducesResponseType<FactoryWarehouseExportDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(
            new GetFactoryWarehouseExportByIdQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.FactoryWarehouseExportCreate)]
    [ProducesResponseType<FactoryWarehouseExportCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateFactoryWarehouseExportRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new CreateFactoryWarehouseExportCommand(
            request.ExportType,
            request.ReferenceRequestId,
            request.ReceiverOrReceivingUnit,
            request.Notes,
            request.Lines,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.ExportId }, result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.FactoryWarehouseExportUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateFactoryWarehouseExportRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new UpdateFactoryWarehouseExportCommand(
            id,
            request.ExportType,
            request.ReferenceRequestId,
            request.ReceiverOrReceivingUnit,
            request.Notes,
            request.Lines,
            User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.FactoryWarehouseExportDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteFactoryWarehouseExportCommand(id, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

public record CreateFactoryWarehouseExportRequest(
    FactoryExportType ExportType,
    int? ReferenceRequestId,
    string ReceiverOrReceivingUnit,
    string? Notes,
    IReadOnlyList<ExportLineInput> Lines);

public record UpdateFactoryWarehouseExportRequest(
    FactoryExportType ExportType,
    int? ReferenceRequestId,
    string ReceiverOrReceivingUnit,
    string? Notes,
    IReadOnlyList<ExportLineInput> Lines);
