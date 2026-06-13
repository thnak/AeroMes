using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetRemainingVolume;

public record GetRemainingVolumeQuery(DateTime? From, DateTime? To) : IQuery<IReadOnlyList<RemainingVolumeItem>>;

public class GetRemainingVolumeQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetRemainingVolumeQuery, IReadOnlyList<RemainingVolumeItem>>
{
    public Task<IReadOnlyList<RemainingVolumeItem>> HandleAsync(
        GetRemainingVolumeQuery query, CancellationToken ct = default)
        => repo.GetRemainingVolumeAsync(query.From, query.To, ct);
}
