using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetOutputOverTime;

public record GetOutputOverTimeQuery(DateTime From, DateTime To, string Granularity)
    : IQuery<IReadOnlyList<OutputOverTimeItem>>;

public class GetOutputOverTimeQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetOutputOverTimeQuery, IReadOnlyList<OutputOverTimeItem>>
{
    public Task<IReadOnlyList<OutputOverTimeItem>> HandleAsync(
        GetOutputOverTimeQuery query, CancellationToken ct = default)
        => repo.GetOutputOverTimeAsync(query.From, query.To, query.Granularity, ct);
}
