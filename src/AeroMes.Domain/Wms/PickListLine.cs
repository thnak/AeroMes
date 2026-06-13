using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class PickListLine : Entity
{
    public long PickLineId { get; private set; }
    public int PickListId { get; private set; }
    public int ShipmentLineId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public string LotNumber { get; private set; } = string.Empty;
    public int? BinId { get; private set; }
    public int LocationId { get; private set; }
    public decimal RequiredQty { get; private set; }
    public decimal PickedQty { get; private set; }
    public bool IsConfirmed { get; private set; }
    public int PickSequence { get; private set; }

    private PickListLine() { }

    internal static PickListLine Create(
        int pickListId,
        int shipmentLineId,
        string productCode,
        string lotNumber,
        int locationId,
        int? binId,
        decimal requiredQty,
        int pickSequence)
    {
        return new PickListLine
        {
            PickListId = pickListId,
            ShipmentLineId = shipmentLineId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            LotNumber = lotNumber.Trim(),
            LocationId = locationId,
            BinId = binId,
            RequiredQty = requiredQty,
            PickSequence = pickSequence,
        };
    }

    internal void Confirm(decimal pickedQty)
    {
        if (IsConfirmed)
            throw new DomainException($"Dòng lấy hàng #{PickLineId} đã được xác nhận.");
        if (pickedQty < 0)
            throw new DomainException("Số lượng lấy không được âm.");
        PickedQty = pickedQty;
        IsConfirmed = true;
    }
}
