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
    public async Task<ActionResult<ApiResponse<List<WorkOrderDto>>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] int? workCenterId,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetWorkOrdersQuery(status, workCenterId), ct);
        return Ok(new ApiResponse<List<WorkOrderDto>>(true, "OK", result));
    }

    [HttpPost("{id:int}/start")]
    public async Task<ActionResult<ApiResponse<StartWorkOrderResult>>> Start(
        int id,
        [FromBody] StartWorkOrderRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new StartWorkOrderCommand(id, request.OperatorId, request.MachineCode, request.Timestamp), ct);
        return Ok(new ApiResponse<StartWorkOrderResult>(true,
            $"Work Order started successfully.", result));
    }
}

public record StartWorkOrderRequest(string OperatorId, string MachineCode, DateTime Timestamp);
