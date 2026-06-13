using AeroMes.Application.Common;
using AeroMes.Application.Master.WorkCalendars.Commands.CreateWorkCalendar;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkCalendars.Commands.UpdateWorkCalendar;

public record UpdateWorkCalendarCommand(
    int WorkCalendarId,
    string Name,
    string? Description,
    bool IsActive,
    IReadOnlyList<CalendarDayInput> Days,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
