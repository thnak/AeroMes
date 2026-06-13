using AeroMes.Application.Common;
using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetActiveHolds;

public sealed record GetActiveHoldsQuery(
    string? LotNumber,
    string? HoldReason,
    int Page = 1,
    int PageSize = 20)
    : IQuery<PagedResult<LotHoldDto>>;
