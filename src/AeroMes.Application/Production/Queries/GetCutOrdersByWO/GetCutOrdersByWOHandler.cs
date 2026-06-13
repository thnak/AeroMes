using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetCutOrdersByWO;

public class GetCutOrdersByWOHandler(ICutOrderRepository repo)
    : IQueryHandler<GetCutOrdersByWOQuery, IReadOnlyList<CutOrderDto>>
{
    public Task<IReadOnlyList<CutOrderDto>> HandleAsync(GetCutOrdersByWOQuery query, CancellationToken ct)
        => repo.GetByWOAsync(query.WOID, ct);
}
