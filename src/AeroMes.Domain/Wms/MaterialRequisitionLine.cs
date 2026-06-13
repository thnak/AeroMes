using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class MaterialRequisitionLine : Entity
{
    public int LineId { get; private set; }
    public int RequisitionId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public decimal RequestedQuantity { get; private set; }
    public int WarehouseId { get; private set; }
    public decimal? ActualIssuedQuantity { get; private set; }
    public string? Notes { get; private set; }

    private MaterialRequisitionLine() { }

    internal static MaterialRequisitionLine Create(
        int requisitionId,
        string productCode,
        string unitOfMeasure,
        decimal requestedQuantity,
        int warehouseId,
        string? notes)
    {
        return new MaterialRequisitionLine
        {
            RequisitionId = requisitionId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            UnitOfMeasure = unitOfMeasure.Trim(),
            RequestedQuantity = requestedQuantity,
            WarehouseId = warehouseId,
            Notes = notes?.Trim(),
        };
    }

    internal void SetActualIssuedQuantity(decimal qty)
    {
        if (qty < 0)
            throw new DomainException("Số lượng thực xuất không được âm.");
        ActualIssuedQuantity = qty;
    }
}
