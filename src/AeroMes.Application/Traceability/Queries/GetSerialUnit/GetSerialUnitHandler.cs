using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetSerialUnit;

public class GetSerialUnitHandler(ISerialUnitRepository repo)
    : IQueryHandler<GetSerialUnitQuery, SerialUnitDetailDto?>
{
    public Task<SerialUnitDetailDto?> HandleAsync(GetSerialUnitQuery query, CancellationToken ct)
        => repo.GetDetailAsync(query.SerialNumber, ct);
}
