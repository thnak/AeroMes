using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetAffectedSerialsFromLot;

public class GetAffectedSerialsFromLotHandler(ISerialUnitRepository repo)
    : IQueryHandler<GetAffectedSerialsFromLotQuery, IReadOnlyList<SerialUnitDto>>
{
    public Task<IReadOnlyList<SerialUnitDto>> HandleAsync(GetAffectedSerialsFromLotQuery query, CancellationToken ct)
        => repo.GetByComponentLotAsync(query.ComponentLotNumber, ct);
}
