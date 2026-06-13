using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Quality.InspectionRequests.Commands.CreateInspectionRequest;
using AeroMes.Application.Quality.InspectionRequests.Commands.DeleteInspectionRequest;
using AeroMes.Application.Quality.InspectionRequests.Commands.UpdateInspectionRequest;
using AeroMes.Application.Quality.InspectionRequests.Commands.UpdateRequestStatus;
using AeroMes.Application.Quality.InspectionRequests.Queries.GetInspectionRequestDetail;
using AeroMes.Application.Quality.InspectionRequests.Queries.GetInspectionRequests;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/quality/inspection-requests")]
[Authorize]
public class QualityInspectionRequestsController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<PagedResult<InspectionRequestDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? status, [FromQuery] string? purpose,
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetInspectionRequestsQuery(status, purpose, from, to, page, pageSize), null, ct));

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.QualityRead)]
    [ProducesResponseType<InspectionRequestDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(int id, CancellationToken ct)
    {
        var detail = await queryMediator.QueryAsync(new GetInspectionRequestDetailQuery(id), null, ct);
        return detail is null ? NotFound() : Ok(detail);
    }

    [HttpPost]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateInspectionRequestRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateInspectionRequestCommand(
                request.RequestNumber, request.RequestDate, request.InspectionPurpose,
                request.RequesterName, request.RequestingDepartment, request.RecipientPerson,
                request.RecipientDepartment, request.InspectionDeadline,
                request.InspectionQuantity, request.Priority, request.Description,
                request.ProductionOrderId, request.StatisticalSheetId, request.InspectionSubject,
                request.SubcontractingOrderId, request.ProductId, User.Identity?.Name),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateInspectionRequestRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateInspectionRequestCommand(
                id, request.RequestDate, request.InspectionPurpose,
                request.RequesterName, request.RequestingDepartment, request.RecipientPerson,
                request.RecipientDepartment, request.InspectionDeadline,
                request.InspectionQuantity, request.Priority, request.Description,
                request.ProductionOrderId, request.StatisticalSheetId, request.InspectionSubject,
                request.SubcontractingOrderId, request.ProductId, User.Identity?.Name),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteInspectionRequestCommand(id, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPatch("{id:int}/status")]
    [RequirePermission(Permissions.QualityWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        int id, [FromBody] UpdateRequestStatusRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateRequestStatusCommand(id, request.Status, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }
}

public record CreateInspectionRequestRequest(
    string RequestNumber,
    DateOnly RequestDate,
    InspectionRequestPurpose InspectionPurpose,
    string RequesterName,
    string RequestingDepartment,
    string RecipientPerson,
    string? RecipientDepartment,
    DateTime InspectionDeadline,
    decimal? InspectionQuantity = null,
    InspectionPriority? Priority = null,
    string? Description = null,
    int? ProductionOrderId = null,
    int? StatisticalSheetId = null,
    string? InspectionSubject = null,
    int? SubcontractingOrderId = null,
    int? ProductId = null);

public record UpdateInspectionRequestRequest(
    DateOnly RequestDate,
    InspectionRequestPurpose InspectionPurpose,
    string RequesterName,
    string RequestingDepartment,
    string RecipientPerson,
    string? RecipientDepartment,
    DateTime InspectionDeadline,
    decimal? InspectionQuantity = null,
    InspectionPriority? Priority = null,
    string? Description = null,
    int? ProductionOrderId = null,
    int? StatisticalSheetId = null,
    string? InspectionSubject = null,
    int? SubcontractingOrderId = null,
    int? ProductId = null);

public record UpdateRequestStatusRequest(InspectionRequestStatus Status);
