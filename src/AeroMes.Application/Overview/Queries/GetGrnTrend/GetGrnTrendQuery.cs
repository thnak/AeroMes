using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetGrnTrend;

public record GetGrnTrendQuery(DateOnly From, DateOnly To) : IQuery<IReadOnlyList<GrnTrendDto>>;

public class GetGrnTrendQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetGrnTrendQuery, IReadOnlyList<GrnTrendDto>>
{
    public Task<IReadOnlyList<GrnTrendDto>> HandleAsync(
        GetGrnTrendQuery query, CancellationToken ct = default)
        => repo.GetGrnTrendAsync(query.From, query.To, ct);
}
