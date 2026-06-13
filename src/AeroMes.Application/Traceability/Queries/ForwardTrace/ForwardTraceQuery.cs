using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.ForwardTrace;

public sealed record ForwardTraceQuery(string LotNumber, int MaxDepth = 20)
    : IQuery<LotGenealogyDto>;

public sealed class ForwardTraceHandler(ILotTraceabilityRepository repo)
    : IQueryHandler<ForwardTraceQuery, LotGenealogyDto>
{
    public Task<LotGenealogyDto> HandleAsync(ForwardTraceQuery q, CancellationToken ct) =>
        repo.ForwardTraceAsync(q.LotNumber, Math.Min(q.MaxDepth, 50), ct);
}
