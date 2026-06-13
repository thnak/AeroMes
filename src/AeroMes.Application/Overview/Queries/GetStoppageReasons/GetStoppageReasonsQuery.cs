using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetStoppageReasons;

public record GetStoppageReasonsQuery(DateTime? From, DateTime? To) : IQuery<IReadOnlyList<StoppageReasonItem>>;

public class GetStoppageReasonsQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetStoppageReasonsQuery, IReadOnlyList<StoppageReasonItem>>
{
    public Task<IReadOnlyList<StoppageReasonItem>> HandleAsync(
        GetStoppageReasonsQuery query, CancellationToken ct = default)
        => repo.GetStoppageReasonsAsync(query.From, query.To, ct);
}
