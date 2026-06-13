using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetRacks;

public class GetRacksHandler(IRackRepository repo)
    : IQueryHandler<GetRacksQuery, IReadOnlyList<RackDto>>
{
    public async Task<IReadOnlyList<RackDto>> HandleAsync(GetRacksQuery query, CancellationToken ct)
    {
        var racks = await repo.GetByAisleAsync(query.AisleId, ct);
        return racks.Select(r => new RackDto(r.RackId, r.AisleId, r.RackCode, r.MaxWeightKg)).ToList();
    }
}
