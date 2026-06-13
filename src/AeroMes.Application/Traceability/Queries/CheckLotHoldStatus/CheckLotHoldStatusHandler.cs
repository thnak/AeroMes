using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.CheckLotHoldStatus;

public sealed class CheckLotHoldStatusHandler(ILotHoldRepository repository)
    : IQueryHandler<CheckLotHoldStatusQuery, LotHoldStatusDto>
{
    public Task<LotHoldStatusDto> HandleAsync(
        CheckLotHoldStatusQuery query, CancellationToken ct)
        => repository.GetStatusAsync(query.LotNumber.ToUpperInvariant(), ct);
}
