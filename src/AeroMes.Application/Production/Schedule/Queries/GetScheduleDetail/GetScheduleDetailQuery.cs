using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Schedule.Queries.GetScheduleDetail;

public record GetScheduleDetailQuery(int ScheduleId) : IQuery<ScheduleDetailDto?>;

public class GetScheduleDetailQueryHandler(IProductionScheduleRepository repo)
    : IQueryHandler<GetScheduleDetailQuery, ScheduleDetailDto?>
{
    public Task<ScheduleDetailDto?> HandleAsync(GetScheduleDetailQuery query, CancellationToken ct = default)
        => repo.GetDetailAsync(query.ScheduleId, ct);
}
