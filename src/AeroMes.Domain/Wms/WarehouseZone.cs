using AeroMes.Domain.Common;

namespace AeroMes.Domain.Wms;

public class WarehouseZone : AuditableEntity
{
    public int ZoneId { get; private set; }
    public string ZoneCode { get; private set; } = string.Empty;
    public string ZoneName { get; private set; } = string.Empty;
    public ZoneType ZoneType { get; private set; }
    public int StorageLocationId { get; private set; }
    public int? WarehouseId { get; private set; }
    public TemperatureZone? TemperatureZone { get; private set; }
    public bool IsActive { get; private set; } = true;

    private WarehouseZone() { }

    public static WarehouseZone Create(
        string code,
        string name,
        ZoneType zoneType,
        int storageLocationId,
        int? warehouseId = null,
        TemperatureZone? temperatureZone = null,
        string? createdBy = null)
        => new()
        {
            ZoneCode = code.Trim().ToUpperInvariant(),
            ZoneName = name.Trim(),
            ZoneType = zoneType,
            StorageLocationId = storageLocationId,
            WarehouseId = warehouseId,
            TemperatureZone = temperatureZone,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };

    public void UpdateDetails(string name, ZoneType zoneType, TemperatureZone? temperatureZone, string updatedBy)
    {
        ZoneName = name.Trim();
        ZoneType = zoneType;
        TemperatureZone = temperatureZone;
        Touch(updatedBy);
    }

    public void Activate(string updatedBy) { IsActive = true; Touch(updatedBy); }
    public void Deactivate(string updatedBy) { IsActive = false; Touch(updatedBy); }
}
