using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Production.Commands.RecordMaterialConsumption;
using AeroMes.Application.Production.Queries.GetWorkOrderMaterialConsumption;
using AeroMes.Application.WorkOrders.Commands.StartWorkOrder;
using AeroMes.Application.WorkOrders.Queries.GetWorkOrderDetail;
using AeroMes.Application.WorkOrders.Queries.GetWorkOrders;
using AeroMes.Domain.Production.Repositories;
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
        if (!result.IsFound) return NotFound(result.ErrorMessage);
        return Ok(new ApiResponse<WorkOrderDetailDto>(true, "OK", result.Value!));
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
        var cmdResult = await commandMediator.SendAsync(
            new StartWorkOrderCommand(id, request.OperatorId, request.Timestamp), null, ct);
        if (!cmdResult.IsSuccess) return cmdResult.ToErrorResult();
        return Ok(new ApiResponse<StartWorkOrderResult>(true, "Work Order started successfully.", cmdResult.Value!));
    }

    [HttpGet("{id:int}/material-consumption")]
    [RequirePermission(Permissions.WorkOrderRead)]
    [ProducesResponseType<IReadOnlyList<MaterialConsumptionDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMaterialConsumption(int id, CancellationToken ct)
        => Ok(await queryMediator.QueryAsync(new GetWorkOrderMaterialConsumptionQuery(id), null, ct));

    [HttpPost("{id:int}/material-consumption/{consumptionId:long}")]
    [RequirePermission(Permissions.WorkOrderStart)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordConsumption(
        int id, long consumptionId, [FromBody] RecordConsumptionRequest req, CancellationToken ct)
    {
        var user = User.Identity?.Name ?? "system";
        var result = await commandMediator.SendAsync(
            new RecordMaterialConsumptionCommand(consumptionId, req.LotNumber, req.ActualQty, user, req.LocationId),
            null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }
}

public record StartWorkOrderRequest(string OperatorId, DateTime? Timestamp);
public record RecordConsumptionRequest(string LotNumber, decimal ActualQty, int LocationId);
