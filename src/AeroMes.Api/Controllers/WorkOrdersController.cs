using AeroMes.Api.Auth;
using AeroMes.Application.Common;
using AeroMes.Application.WorkOrders.Commands.StartWorkOrder;
using AeroMes.Application.WorkOrders.Queries.GetWorkOrderDetail;
using AeroMes.Application.WorkOrders.Queries.GetWorkOrders;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/work-orders")]
[Authorize]
public class WorkOrdersController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.WorkOrderRead)]
    [ProducesResponseType<ApiResponse<IReadOnlyList<WorkOrderDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WorkOrderDto>>>> GetAll(
        [FromQuery] string? status,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetWorkOrdersQuery(status), null, ct);
        return Ok(new ApiResponse<IReadOnlyList<WorkOrderDto>>(true, "OK", result));
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.WorkOrderRead)]
    [ProducesResponseType<ApiResponse<WorkOrderDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<WorkOrderDetailDto>>> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetWorkOrderDetailQuery(id), null, ct);
        return Ok(new ApiResponse<WorkOrderDetailDto>(true, "OK", result));
    }

    [HttpPost("{id:int}/start")]
    [RequirePermission(Permissions.WorkOrderStart)]
    [ProducesResponseType<ApiResponse<StartWorkOrderResult>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<StartWorkOrderResult>>> Start(
        int id,
        [FromBody] StartWorkOrderRequest request,
        CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new StartWorkOrderCommand(id, request.OperatorId, request.Timestamp), null, ct);
        return Ok(new ApiResponse<StartWorkOrderResult>(true, "Work Order started successfully.", result));
    }
}

public record StartWorkOrderRequest(string OperatorId, DateTime? Timestamp);
