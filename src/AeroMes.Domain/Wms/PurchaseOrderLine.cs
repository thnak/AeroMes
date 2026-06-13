using AeroMes.Domain.Common;

namespace AeroMes.Domain.Wms;

public class PurchaseOrderLine : Entity
{
    public int PoLineId { get; private set; }
    public int PoId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public decimal OrderedQty { get; private set; }
    public decimal ReceivedQty { get; private set; }
    public decimal? UnitPrice { get; private set; }
    public string? ExpectedLotNumber { get; private set; }

    private PurchaseOrderLine() { }

    internal static PurchaseOrderLine Create(
        int poId, string productCode, decimal orderedQty, decimal? unitPrice, string? expectedLotNumber)
    {
        return new PurchaseOrderLine
        {
            PoId = poId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            OrderedQty = orderedQty,
            ReceivedQty = 0m,
            UnitPrice = unitPrice,
            ExpectedLotNumber = expectedLotNumber?.Trim(),
        };
    }

    internal void AddReceived(decimal qty) => ReceivedQty += qty;
}
