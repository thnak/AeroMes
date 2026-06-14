using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetStockByLocation;

public record GetStockByLocationQuery : IQuery<IReadOnlyList<StockByLocationDto>>;

public class GetStockByLocationQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetStockByLocationQuery, IReadOnlyList<StockByLocationDto>>
{
    public Task<IReadOnlyList<StockByLocationDto>> HandleAsync(
        GetStockByLocationQuery query, CancellationToken ct = default)
        => repo.GetStockByLocationAsync(ct);
}
