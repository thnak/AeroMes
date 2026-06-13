using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.BidirectionalTrace;

public sealed record BidirectionalTraceQuery(string LotNumber, int MaxDepth = 20)
    : IQuery<LotGenealogyDto>;

public sealed class BidirectionalTraceHandler(ILotTraceabilityRepository repo)
    : IQueryHandler<BidirectionalTraceQuery, LotGenealogyDto>
{
    public Task<LotGenealogyDto> HandleAsync(BidirectionalTraceQuery q, CancellationToken ct) =>
        repo.BidirectionalTraceAsync(q.LotNumber, Math.Min(q.MaxDepth, 50), ct);
}
