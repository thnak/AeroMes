using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetCutOrder;

public class GetCutOrderHandler(ICutOrderRepository repo)
    : IQueryHandler<GetCutOrderQuery, CutOrderDto?>
{
    public Task<CutOrderDto?> HandleAsync(GetCutOrderQuery query, CancellationToken ct)
        => repo.GetDetailAsync(query.CutOrderID, ct);
}
