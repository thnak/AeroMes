using AeroMes.Domain.Common;

namespace AeroMes.Domain.Wms;

public class FactoryReceiptLine : Entity
{
    public int LineId { get; private set; }
    public int ReceiptId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public int DestinationWarehouseId { get; private set; }
    public string? SpecificationCode { get; private set; }

    private FactoryReceiptLine() { }

    internal static FactoryReceiptLine Create(
        int receiptId,
        string productCode,
        string unitOfMeasure,
        decimal quantity,
        int destinationWarehouseId,
        string? specificationCode)
    {
        return new FactoryReceiptLine
        {
            ReceiptId = receiptId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            UnitOfMeasure = unitOfMeasure.Trim(),
            Quantity = quantity,
            DestinationWarehouseId = destinationWarehouseId,
            SpecificationCode = specificationCode?.Trim(),
        };
    }
}
