using AeroMes.Application.Common;
using AeroMes.Application.WorkOrders.Commands.StartWorkOrder;
using AeroMes.Application.WorkOrders.Queries.GetWorkOrders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/work-orders")]
[Authorize]
public class WorkOrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WorkOrderDto>>>> GetAll(
        [FromQuery] string? status,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetWorkOrdersQuery(status), ct);
        return Ok(new ApiResponse<IReadOnlyList<WorkOrderDto>>(true, "OK", result));
    }

    [HttpPost("{id:int}/start")]
    public async Task<ActionResult<ApiResponse<StartWorkOrderResult>>> Start(
        int id,
        [FromBody] StartWorkOrderRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new StartWorkOrderCommand(id, request.OperatorId, request.Timestamp), ct);
        return Ok(new ApiResponse<StartWorkOrderResult>(true, "Work Order started successfully.", result));
    }
}

public record StartWorkOrderRequest(string OperatorId, DateTime? Timestamp);
