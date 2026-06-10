using AeroMes.Domain.Master;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.WorkCalendars.Queries.GetWorkCalendarById;

public record GetWorkCalendarByIdQuery(int WorkCalendarId) : IQuery<WorkCalendarDetailDto?>;

public record CalendarShiftInfoDto(int CalendarShiftId, int WorkShiftId, string ShiftCode, string ShiftName, int Sequence);
public record CalendarDayDto(DayOfWeek DayOfWeek, bool IsWorkingDay, IReadOnlyList<CalendarShiftInfoDto> Shifts);
public record CalendarExceptionDto(int CalendarExceptionId, DateOnly Date, CalendarExceptionType ExceptionType, int? WorkShiftId);

public record WorkCalendarDetailDto(
    int WorkCalendarId,
    string CalendarCode,
    string CalendarName,
    string? Description,
    bool IsActive,
    IReadOnlyList<CalendarDayDto> Days,
    IReadOnlyList<CalendarExceptionDto> Exceptions);
