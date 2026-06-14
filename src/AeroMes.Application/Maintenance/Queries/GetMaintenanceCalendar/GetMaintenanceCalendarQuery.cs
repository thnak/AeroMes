using AeroMes.Domain.Maintenance.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Maintenance.Queries.GetMaintenanceCalendar;

public record GetMaintenanceCalendarQuery(DateTime From, DateTime To, string? MachineCode)
    : IQuery<IReadOnlyList<MwoCalendarDto>>;

public class GetMaintenanceCalendarHandler(IMaintenancePlanRepository repo)
    : IQueryHandler<GetMaintenanceCalendarQuery, IReadOnlyList<MwoCalendarDto>>
{
    public Task<IReadOnlyList<MwoCalendarDto>> HandleAsync(
        GetMaintenanceCalendarQuery query, CancellationToken ct)
        => repo.GetCalendarAsync(query.From, query.To, query.MachineCode, ct);
}
