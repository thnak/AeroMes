using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.BackwardTrace;

public sealed record BackwardTraceQuery(string LotNumber, int MaxDepth = 20)
    : IQuery<LotGenealogyDto>;
