using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Master.WorkCalendars.Commands.AddCalendarException;
using AeroMes.Application.Master.WorkCalendars.Commands.CreateWorkCalendar;
using AeroMes.Application.Master.WorkCalendars.Commands.DeleteWorkCalendar;
using AeroMes.Application.Master.WorkCalendars.Commands.RemoveCalendarException;
using AeroMes.Application.Master.WorkCalendars.Commands.UpdateWorkCalendar;
using AeroMes.Application.Master.WorkCalendars.Queries.GetWorkCalendarById;
using AeroMes.Application.Master.WorkCalendars.Queries.GetWorkCalendars;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/work-calendars")]
[Authorize]
public class WorkCalendarsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<WorkCalendarDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetWorkCalendarsQuery(activeOnly), null, ct));

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<WorkCalendarDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetWorkCalendarByIdQuery(id), null, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<WorkCalendarCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateWorkCalendarRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateWorkCalendarCommand(req.Code, req.Name, req.Description,
                req.Days.Select(d => new CalendarDayInput(d.DayOfWeek, d.IsWorkingDay,
                    d.Shifts.Select(s => new CalendarShiftInput(s.WorkShiftId, s.Sequence)).ToList())).ToList(),
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        var id = result.Value!;
        return CreatedAtAction(nameof(GetById), new { id }, new WorkCalendarCreatedResult(id));
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkCalendarRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateWorkCalendarCommand(id, req.Name, req.Description, req.IsActive,
                req.Days.Select(d => new CalendarDayInput(d.DayOfWeek, d.IsWorkingDay,
                    d.Shifts.Select(s => new CalendarShiftInput(s.WorkShiftId, s.Sequence)).ToList())).ToList(),
                User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new DeleteWorkCalendarCommand(id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/exceptions")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<CalendarExceptionCreatedResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddException(int id, [FromBody] AddCalendarExceptionRequest req, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new AddCalendarExceptionCommand(id, req.Date, req.ExceptionType, req.WorkShiftId, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        var exId = result.Value!;
        return CreatedAtAction(nameof(GetById), new { id }, new CalendarExceptionCreatedResult(exId));
    }

    [HttpDelete("{id:int}/exceptions/{exceptionId:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveException(int id, int exceptionId, CancellationToken ct)
    {
        await commandMediator.SendAsync(
            new RemoveCalendarExceptionCommand(id, exceptionId, User.FindFirst(ClaimTypes.NameIdentifier)?.Value), null, ct);
        return NoContent();
    }
}

public record CalendarShiftRequest(int WorkShiftId, int Sequence);
public record CalendarDayRequest(DayOfWeek DayOfWeek, bool IsWorkingDay, IReadOnlyList<CalendarShiftRequest> Shifts);
public record CreateWorkCalendarRequest(
    string Code, string Name, string? Description,
    IReadOnlyList<CalendarDayRequest> Days);
public record UpdateWorkCalendarRequest(
    string Name, string? Description, bool IsActive,
    IReadOnlyList<CalendarDayRequest> Days);
public record AddCalendarExceptionRequest(DateOnly Date, CalendarExceptionType ExceptionType, int? WorkShiftId);
public record WorkCalendarCreatedResult(int WorkCalendarId);
public record CalendarExceptionCreatedResult(int CalendarExceptionId);
