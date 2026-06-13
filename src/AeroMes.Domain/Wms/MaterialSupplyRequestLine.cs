using AeroMes.Domain.Common;

namespace AeroMes.Domain.Wms;

public class MaterialSupplyRequestLine : Entity
{
    public int LineId { get; private set; }
    public int RequestId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public decimal RequestedQuantity { get; private set; }
    public int? WarehouseId { get; private set; }
    public string? Notes { get; private set; }

    private MaterialSupplyRequestLine() { }

    internal static MaterialSupplyRequestLine Create(
        int requestId,
        string productCode,
        string unitOfMeasure,
        decimal requestedQuantity,
        int? warehouseId,
        string? notes)
    {
        return new MaterialSupplyRequestLine
        {
            RequestId = requestId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            UnitOfMeasure = unitOfMeasure.Trim(),
            RequestedQuantity = requestedQuantity,
            WarehouseId = warehouseId,
            Notes = notes?.Trim(),
        };
    }
}
