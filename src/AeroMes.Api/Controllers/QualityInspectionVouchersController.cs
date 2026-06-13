using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Quality.InspectionVouchers.Commands.AddVoucherDefect;
using AeroMes.Application.Quality.InspectionVouchers.Commands.CreateInspectionVoucher;
using AeroMes.Application.Quality.InspectionVouchers.Commands.UpdateVoucherStatus;
using AeroMes.Application.Quality.InspectionVouchers.Queries.GetInspectionVouchers;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/quality/inspection-vouchers")]
[Authorize]
public class QualityInspectionVouchersController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpPost]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateInspectionVoucherRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateInspectionVoucherCommand(
                request.VoucherNumber, request.VoucherName, request.InspectionType,
                request.InspectorName, request.InspectionDate,
                request.LinkedRequestId, request.ProductionOrderId,
                request.SampleQuantity, User.Identity?.Name),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpGet]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<PagedResult<QualityInspectionVoucherDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? status,
        [FromQuery] string? inspectionType,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetInspectionVouchersQuery(status, inspectionType, from, to, page, pageSize),
            null, ct));

    [HttpPost("{id:int}/defects")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddDefect(
        int id, [FromBody] AddDefectRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AddVoucherDefectCommand(id, request.DefectCodeId, request.DefectName,
                request.Quantity, User.Identity?.Name),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPost("{id:int}/status")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        int id, [FromBody] UpdateVoucherStatusRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateVoucherStatusCommand(id, request.Action, request.Conclusion, User.Identity?.Name),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }
}

public record CreateInspectionVoucherRequest(
    string VoucherNumber,
    string VoucherName,
    InspectionVoucherType InspectionType,
    string InspectorName,
    DateOnly InspectionDate,
    int? LinkedRequestId,
    int? ProductionOrderId,
    decimal SampleQuantity);

public record AddDefectRequest(int DefectCodeId, string DefectName, decimal Quantity);

public record UpdateVoucherStatusRequest(string Action, InspectionConclusion? Conclusion);
