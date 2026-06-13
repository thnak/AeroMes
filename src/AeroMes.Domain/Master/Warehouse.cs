using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public enum WarehouseType { RawMaterial, WIP, FinishedGoods, General }
public enum IntegrationSource { Manual, AMISWarehouse, AMISAccounting }

public class Warehouse : AuditableEntity
{
    public int WarehouseId { get; private set; }
    public string WarehouseCode { get; private set; } = string.Empty;
    public string WarehouseName { get; private set; } = string.Empty;
    public string? Address { get; private set; }
    public WarehouseType WarehouseType { get; private set; }
    public IntegrationSource IntegrationSource { get; private set; } = IntegrationSource.Manual;
    public bool IsActive { get; private set; } = true;

    private Warehouse() { }

    public static Warehouse Create(
        string code,
        string name,
        WarehouseType warehouseType,
        string? address = null,
        IntegrationSource integrationSource = IntegrationSource.Manual,
        string? createdBy = null)
    {
        return new Warehouse
        {
            WarehouseCode = code.Trim().ToUpperInvariant(),
            WarehouseName = name.Trim(),
            WarehouseType = warehouseType,
            Address = address?.Trim(),
            IntegrationSource = integrationSource,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateDetails(string name, string? address, WarehouseType warehouseType, string updatedBy)
    {
        WarehouseName = name.Trim();
        Address = address?.Trim();
        WarehouseType = warehouseType;
        Touch(updatedBy);
    }

    public void Activate(string updatedBy)
    {
        IsActive = true;
        Touch(updatedBy);
    }

    public void Deactivate(string updatedBy)
    {
        IsActive = false;
        Touch(updatedBy);
    }
}
