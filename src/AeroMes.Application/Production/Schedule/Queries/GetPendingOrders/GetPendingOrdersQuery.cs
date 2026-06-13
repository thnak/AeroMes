using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Schedule.Queries.GetPendingOrders;

public record GetPendingOrdersQuery(DateTime PeriodStart, DateTime PeriodEnd, int? ScheduleId)
    : IQuery<IReadOnlyList<PendingOrderDto>>;

public class GetPendingOrdersQueryHandler(IProductionScheduleRepository repo)
    : IQueryHandler<GetPendingOrdersQuery, IReadOnlyList<PendingOrderDto>>
{
    public Task<IReadOnlyList<PendingOrderDto>> HandleAsync(
        GetPendingOrdersQuery query, CancellationToken ct = default)
        => repo.GetPendingOrdersAsync(query.PeriodStart, query.PeriodEnd, query.ScheduleId, ct);
}
