using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetOutputByStage;

public record GetOutputByStageQuery(DateTime? From, DateTime? To) : IQuery<IReadOnlyList<OutputByStageItem>>;

public class GetOutputByStageQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetOutputByStageQuery, IReadOnlyList<OutputByStageItem>>
{
    public Task<IReadOnlyList<OutputByStageItem>> HandleAsync(
        GetOutputByStageQuery query, CancellationToken ct = default)
        => repo.GetOutputByStageAsync(query.From, query.To, ct);
}
