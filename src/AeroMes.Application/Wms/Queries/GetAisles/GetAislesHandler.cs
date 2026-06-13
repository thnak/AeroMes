using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetAisles;

public class GetAislesHandler(IAisleRepository repo)
    : IQueryHandler<GetAislesQuery, IReadOnlyList<AisleDto>>
{
    public async Task<IReadOnlyList<AisleDto>> HandleAsync(GetAislesQuery query, CancellationToken ct)
    {
        var aisles = await repo.GetByZoneAsync(query.ZoneId, ct);
        return aisles.Select(a => new AisleDto(a.AisleId, a.ZoneId, a.AisleCode, a.PickSequence)).ToList();
    }
}
