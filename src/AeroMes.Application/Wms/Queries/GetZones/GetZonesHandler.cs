using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetZones;

public class GetZonesHandler(IWarehouseZoneRepository repo)
    : IQueryHandler<GetZonesQuery, IReadOnlyList<ZoneDto>>
{
    public async Task<IReadOnlyList<ZoneDto>> HandleAsync(GetZonesQuery query, CancellationToken ct)
    {
        var zones = await repo.GetAllAsync(query.StorageLocationId, query.ActiveOnly, ct);
        return zones.Select(z => new ZoneDto(
            z.ZoneId, z.ZoneCode, z.ZoneName,
            z.ZoneType.ToString(), z.StorageLocationId, z.WarehouseId,
            z.TemperatureZone?.ToString(), z.IsActive, z.CreatedAt)).ToList();
    }
}
