using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.WorkCalendars.Queries.GetWorkCalendars;

public record GetWorkCalendarsQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<WorkCalendarDto>>;

public record WorkCalendarDto(
    int WorkCalendarId,
    string CalendarCode,
    string CalendarName,
    string? Description,
    bool IsActive);
