using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetPackagingOrders;

public class GetPackagingOrdersHandler(IPackagingRepository repo)
    : IQueryHandler<GetPackagingOrdersQuery, IReadOnlyList<PackagingOrderDto>>
{
    public Task<IReadOnlyList<PackagingOrderDto>> HandleAsync(GetPackagingOrdersQuery query, CancellationToken ct)
        => repo.GetOrdersAsync(query.WOID, query.Status, ct);
}
