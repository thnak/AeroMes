using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Production.Commands.ApproveMaterialPurchaseRequest;
using AeroMes.Application.Production.Commands.CreateMaterialPurchaseRequest;
using AeroMes.Application.Production.Commands.SubmitMaterialPurchaseRequest;
using AeroMes.Application.Production.Queries.GetMaterialPurchaseRequests;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/purchase-requests")]
[Authorize]
public class MaterialPurchaseRequestsController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpPost]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePurchaseRequestRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateMaterialPurchaseRequestCommand(
                request.Requestor, request.RequestingUnit, request.Deadline,
                request.ProcurementPurpose, request.SourceType,
                request.SourceReferenceId, request.SalesOrderCode, request.Lines),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpGet]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<PagedResult<MaterialPurchaseRequestDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] PurchaseRequestStatus? status,
        [FromQuery] PurchaseRequestSourceType? sourceType,
        [FromQuery] string? requestingUnit,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(
            new GetMaterialPurchaseRequestsQuery(status, sourceType, requestingUnit, from, to, page, pageSize),
            null, ct));

    [HttpPost("{id:int}/submit")]
    [RequirePermission(Permissions.ProductionSubmitOutput)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Submit(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new SubmitMaterialPurchaseRequestCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPost("{id:int}/approve")]
    [RequirePermission(Permissions.ProductionPlanningWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(
        int id, [FromBody] ApproveRequestBody body, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new ApproveMaterialPurchaseRequestCommand(id, body.IsApproved), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }
}

public record CreatePurchaseRequestRequest(
    string Requestor,
    string? RequestingUnit,
    DateOnly? Deadline,
    string? ProcurementPurpose,
    PurchaseRequestSourceType SourceType,
    int? SourceReferenceId,
    string? SalesOrderCode,
    IReadOnlyList<PurchaseRequestLineInput> Lines);

public record ApproveRequestBody(bool IsApproved);
