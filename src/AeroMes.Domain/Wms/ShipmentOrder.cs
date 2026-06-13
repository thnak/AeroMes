using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class ShipmentOrder : AuditableEntity
{
    public int ShipmentId { get; private set; }
    public string ShipmentCode { get; private set; } = string.Empty;
    public int? SoId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public DateOnly RequestedShipDate { get; private set; }
    public ShipmentStatus Status { get; private set; } = ShipmentStatus.Draft;
    public string? CarrierName { get; private set; }
    public string? TrackingNumber { get; private set; }

    private readonly List<ShipmentLine> _lines = [];
    public IReadOnlyList<ShipmentLine> Lines => _lines.AsReadOnly();

    private ShipmentOrder() { }

    public static ShipmentOrder Create(
        string shipmentCode,
        int? soId,
        string customerName,
        DateOnly requestedShipDate,
        string? createdBy)
    {
        return new ShipmentOrder
        {
            ShipmentCode = shipmentCode.Trim().ToUpperInvariant(),
            SoId = soId,
            CustomerName = customerName.Trim(),
            RequestedShipDate = requestedShipDate,
            Status = ShipmentStatus.Draft,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public ShipmentLine AddLine(string productCode, decimal orderedQty)
    {
        if (Status != ShipmentStatus.Draft)
            throw new DomainException($"Lệnh xuất hàng '{ShipmentCode}' phải ở trạng thái Nháp để thêm dòng.");
        var line = ShipmentLine.Create(ShipmentId, productCode, orderedQty);
        _lines.Add(line);
        return line;
    }

    public void RemoveLine(int lineId)
    {
        if (Status != ShipmentStatus.Draft)
            throw new DomainException($"Lệnh xuất hàng '{ShipmentCode}' phải ở trạng thái Nháp để xóa dòng.");
        var line = _lines.FirstOrDefault(l => l.LineId == lineId)
            ?? throw new DomainException($"Không tìm thấy dòng #{lineId} trong lệnh xuất hàng '{ShipmentCode}'.");
        _lines.Remove(line);
    }

    public void StartPicking(string? updatedBy)
    {
        if (Status != ShipmentStatus.Draft)
            throw new DomainException($"Lệnh xuất hàng '{ShipmentCode}' phải ở trạng thái Nháp để bắt đầu lấy hàng.");
        if (_lines.Count == 0)
            throw new DomainException($"Lệnh xuất hàng '{ShipmentCode}' phải có ít nhất một dòng.");
        Status = ShipmentStatus.Picking;
        Touch(updatedBy);
    }

    public void MarkPacked(string? updatedBy)
    {
        if (Status != ShipmentStatus.Picking)
            throw new DomainException($"Lệnh xuất hàng '{ShipmentCode}' phải ở trạng thái Đang lấy hàng để đánh dấu đã đóng gói.");
        Status = ShipmentStatus.Packed;
        Touch(updatedBy);
    }

    public void Dispatch(string? carrierName, string? trackingNumber, string? dispatchedBy)
    {
        if (Status != ShipmentStatus.Picking && Status != ShipmentStatus.Packed)
            throw new DomainException($"Lệnh xuất hàng '{ShipmentCode}' phải ở trạng thái Đang lấy hàng hoặc Đã đóng gói để xuất hàng.");
        CarrierName = carrierName?.Trim();
        TrackingNumber = trackingNumber?.Trim();
        Status = ShipmentStatus.Dispatched;
        Touch(dispatchedBy);
    }

    public void Cancel(string? cancelledBy)
    {
        if (Status == ShipmentStatus.Dispatched)
            throw new DomainException($"Lệnh xuất hàng '{ShipmentCode}' đã được xuất, không thể huỷ.");
        SoftDelete(cancelledBy);
        Status = ShipmentStatus.Cancelled;
    }
}
