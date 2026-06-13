using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetTopProductsByErrorRate;

public record GetTopProductsByErrorRateQuery(DateTime? From, DateTime? To, int TopN = 10)
    : IQuery<IReadOnlyList<TopProductByErrorRateItem>>;

public class GetTopProductsByErrorRateQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetTopProductsByErrorRateQuery, IReadOnlyList<TopProductByErrorRateItem>>
{
    public Task<IReadOnlyList<TopProductByErrorRateItem>> HandleAsync(
        GetTopProductsByErrorRateQuery query, CancellationToken ct = default)
        => repo.GetTopProductsByErrorRateAsync(query.From, query.To, query.TopN, ct);
}
