using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetHoldHistory;

public sealed record GetHoldHistoryQuery(string LotNumber)
    : IQuery<IReadOnlyList<LotHoldDto>>;
