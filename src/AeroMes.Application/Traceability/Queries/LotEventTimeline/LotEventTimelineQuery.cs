using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.LotEventTimeline;

public sealed record LotEventTimelineQuery(string LotNumber, DateTime? From, DateTime? To)
    : IQuery<IReadOnlyList<LotEventDto>>;

public sealed class LotEventTimelineHandler(ILotTraceabilityRepository repo)
    : IQueryHandler<LotEventTimelineQuery, IReadOnlyList<LotEventDto>>
{
    public Task<IReadOnlyList<LotEventDto>> HandleAsync(LotEventTimelineQuery q, CancellationToken ct) =>
        repo.GetEventTimelineAsync(q.LotNumber, q.From, q.To, ct);
}
