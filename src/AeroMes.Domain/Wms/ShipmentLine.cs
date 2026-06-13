using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class ShipmentLine : Entity
{
    public int LineId { get; private set; }
    public int ShipmentId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public decimal OrderedQty { get; private set; }
    public decimal PickedQty { get; private set; }
    public decimal PackedQty { get; private set; }

    private ShipmentLine() { }

    internal static ShipmentLine Create(int shipmentId, string productCode, decimal orderedQty)
    {
        if (orderedQty <= 0)
            throw new DomainException("Số lượng đặt hàng phải lớn hơn 0.");
        return new ShipmentLine
        {
            ShipmentId = shipmentId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            OrderedQty = orderedQty,
        };
    }

    public void RecordPicked(decimal qty)
    {
        PickedQty += qty;
    }

    public void RecordPacked(decimal qty)
    {
        if (PackedQty + qty > PickedQty)
            throw new DomainException($"Số lượng đóng gói ({PackedQty + qty}) vượt quá số lượng đã lấy ({PickedQty}).");
        PackedQty += qty;
    }
}
