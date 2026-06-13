using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetZones;

public record GetZonesQuery(int? StorageLocationId = null, bool ActiveOnly = true)
    : IQuery<IReadOnlyList<ZoneDto>>;

public record ZoneDto(
    int ZoneId,
    string ZoneCode,
    string ZoneName,
    string ZoneType,
    int StorageLocationId,
    int? WarehouseId,
    string? TemperatureZone,
    bool IsActive,
    DateTime CreatedAt);
