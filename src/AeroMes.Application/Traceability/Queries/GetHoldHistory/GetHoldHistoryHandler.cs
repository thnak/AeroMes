using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetHoldHistory;

public sealed class GetHoldHistoryHandler(ILotHoldRepository repository)
    : IQueryHandler<GetHoldHistoryQuery, IReadOnlyList<LotHoldDto>>
{
    public Task<IReadOnlyList<LotHoldDto>> HandleAsync(
        GetHoldHistoryQuery query, CancellationToken ct)
        => repository.GetHistoryAsync(query.LotNumber.ToUpperInvariant(), ct);
}
