using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Common;
using AeroMes.Application.Production.Schedule.Commands.AssignPrimaryCapacity;
using AeroMes.Application.Production.Schedule.Commands.AutoArrangeSchedule;
using AeroMes.Application.Production.Schedule.Commands.CompleteSchedule;
using AeroMes.Application.Production.Schedule.Commands.CreateSchedule;
using AeroMes.Application.Production.Schedule.Commands.DeleteSchedule;
using AeroMes.Application.Production.Schedule.Commands.UpdateScheduleLines;
using AeroMes.Application.Production.Schedule.Queries.GetPendingOrders;
using AeroMes.Application.Production.Schedule.Queries.GetScheduleDetail;
using AeroMes.Application.Production.Schedule.Queries.GetScheduleList;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/production-schedules")]
[Authorize]
public class ProductionSchedulesController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<PagedResult<ScheduleListDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetScheduleListQuery(status, from, to, page, pageSize), null, ct));

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<ScheduleDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetScheduleDetailQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("pending-orders")]
    [RequirePermission(Permissions.ProductionRead)]
    [ProducesResponseType<IReadOnlyList<PendingOrderDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingOrders(
        [FromQuery] DateTime periodStart,
        [FromQuery] DateTime periodEnd,
        [FromQuery] int? scheduleId,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetPendingOrdersQuery(periodStart, periodEnd, scheduleId), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateScheduleRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateScheduleCommand(
                request.ScheduleName, request.FacilityCode,
                request.PeriodStart, request.PeriodEnd, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok(result.Value);
    }

    [HttpPut("{id:int}/lines")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateLines(
        int id, [FromBody] UpdateScheduleLinesRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateScheduleLinesCommand(id, request.Lines, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPost("{id:int}/auto-arrange")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AutoArrange(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AutoArrangeScheduleCommand(id, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPost("{id:int}/assign-primary/{orderId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignPrimary(int id, int orderId, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AssignPrimaryCapacityCommand(id, orderId, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPut("{id:int}/complete")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Complete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CompleteScheduleCommand(id, User.Identity?.Name, SaveAsDraft: false), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpPut("{id:int}/draft")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveAsDraft(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CompleteScheduleCommand(id, User.Identity?.Name, SaveAsDraft: true), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new DeleteScheduleCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }
}

public record CreateScheduleRequest(
    string? ScheduleName, string? FacilityCode,
    DateTime PeriodStart, DateTime PeriodEnd);

public record UpdateScheduleLinesRequest(IReadOnlyList<ScheduleLineInput> Lines);
