using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetSerialComponentLots;

public class GetSerialComponentLotsHandler(ISerialUnitRepository repo)
    : IQueryHandler<GetSerialComponentLotsQuery, IReadOnlyList<SerialLotLineageDto>>
{
    public Task<IReadOnlyList<SerialLotLineageDto>> HandleAsync(GetSerialComponentLotsQuery query, CancellationToken ct)
        => repo.GetComponentLotsAsync(query.SerialNumber, ct);
}
