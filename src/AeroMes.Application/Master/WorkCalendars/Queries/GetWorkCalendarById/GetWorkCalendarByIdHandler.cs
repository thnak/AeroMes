using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.WorkCalendars.Queries.GetWorkCalendarById;

public class GetWorkCalendarByIdHandler(IWorkCalendarRepository repo)
    : IQueryHandler<GetWorkCalendarByIdQuery, WorkCalendarDetailDto?>
{
    public async Task<WorkCalendarDetailDto?> HandleAsync(GetWorkCalendarByIdQuery query, CancellationToken ct)
    {
        var cal = await repo.GetByIdWithDetailsAsync(query.WorkCalendarId, ct);
        if (cal is null) return null;

        var days = cal.Days
            .OrderBy(d => d.DayOfWeek)
            .Select(d => new CalendarDayDto(
                d.DayOfWeek, d.IsWorkingDay,
                d.Shifts.OrderBy(s => s.Sequence)
                    .Select(s => new CalendarShiftInfoDto(
                        s.CalendarShiftId, s.WorkShiftId,
                        s.WorkShift?.ShiftCode ?? string.Empty,
                        s.WorkShift?.ShiftName ?? string.Empty,
                        s.Sequence))
                    .ToList()))
            .ToList();

        var exceptions = cal.Exceptions
            .OrderBy(e => e.Date)
            .Select(e => new CalendarExceptionDto(e.CalendarExceptionId, e.Date, e.ExceptionType, e.WorkShiftId))
            .ToList();

        return new WorkCalendarDetailDto(
            cal.WorkCalendarId, cal.CalendarCode, cal.CalendarName,
            cal.Description, cal.IsActive, days, exceptions);
    }
}
