using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.BackwardTrace;

public sealed class BackwardTraceHandler(ILotTraceabilityRepository repo)
    : IQueryHandler<BackwardTraceQuery, LotGenealogyDto>
{
    public Task<LotGenealogyDto> HandleAsync(BackwardTraceQuery q, CancellationToken ct) =>
        repo.BackwardTraceAsync(q.LotNumber, Math.Min(q.MaxDepth, 50), ct);
}
