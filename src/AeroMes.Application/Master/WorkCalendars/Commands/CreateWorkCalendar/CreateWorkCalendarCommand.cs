using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkCalendars.Commands.CreateWorkCalendar;

public record CalendarShiftInput(int WorkShiftId, int Sequence);
public record CalendarDayInput(DayOfWeek DayOfWeek, bool IsWorkingDay, IReadOnlyList<CalendarShiftInput> Shifts);

public record CreateWorkCalendarCommand(
    string Code,
    string Name,
    string? Description,
    IReadOnlyList<CalendarDayInput> Days,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
