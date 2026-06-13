using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetTopProductsByVolume;

public record GetTopProductsByVolumeQuery(DateTime? From, DateTime? To, int TopN = 10)
    : IQuery<IReadOnlyList<TopProductByVolumeItem>>;

public class GetTopProductsByVolumeQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetTopProductsByVolumeQuery, IReadOnlyList<TopProductByVolumeItem>>
{
    public Task<IReadOnlyList<TopProductByVolumeItem>> HandleAsync(
        GetTopProductsByVolumeQuery query, CancellationToken ct = default)
        => repo.GetTopProductsByVolumeAsync(query.From, query.To, query.TopN, ct);
}
