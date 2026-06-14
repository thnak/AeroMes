using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Schedule.Queries.GetDispatchList;

public record GetDispatchListQuery(int WorkCenterId, DateOnly Date) : IQuery<IReadOnlyList<DispatchItemDto>>;

public class GetDispatchListHandler(IProductionScheduleRepository repo)
    : IQueryHandler<GetDispatchListQuery, IReadOnlyList<DispatchItemDto>>
{
    public Task<IReadOnlyList<DispatchItemDto>> HandleAsync(GetDispatchListQuery q, CancellationToken ct)
        => repo.GetDispatchListAsync(q.WorkCenterId, q.Date, ct);
}
