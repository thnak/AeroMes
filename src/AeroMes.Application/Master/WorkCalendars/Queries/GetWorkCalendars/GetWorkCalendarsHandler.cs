using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.WorkCalendars.Queries.GetWorkCalendars;

public class GetWorkCalendarsHandler(IWorkCalendarRepository repo)
    : IQueryHandler<GetWorkCalendarsQuery, IReadOnlyList<WorkCalendarDto>>
{
    public async Task<IReadOnlyList<WorkCalendarDto>> HandleAsync(GetWorkCalendarsQuery query, CancellationToken ct)
    {
        var list = await repo.GetAllAsync(query.ActiveOnly, ct);
        return list.Select(x => new WorkCalendarDto(
            x.WorkCalendarId, x.CalendarCode, x.CalendarName, x.Description, x.IsActive)).ToList();
    }
}
