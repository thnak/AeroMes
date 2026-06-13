using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class PickList : AuditableEntity
{
    public int PickListId { get; private set; }
    public int ShipmentId { get; private set; }
    public string? AssignedTo { get; private set; }
    public PickListStatus Status { get; private set; } = PickListStatus.Pending;
    public DateTime? CompletedAt { get; private set; }

    private readonly List<PickListLine> _lines = [];
    public IReadOnlyList<PickListLine> Lines => _lines.AsReadOnly();

    private PickList() { }

    public static PickList Create(int shipmentId, string? assignedTo, string? createdBy)
    {
        return new PickList
        {
            ShipmentId = shipmentId,
            AssignedTo = assignedTo?.Trim(),
            Status = PickListStatus.Pending,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public PickListLine AddLine(
        int shipmentLineId,
        string productCode,
        string lotNumber,
        int locationId,
        int? binId,
        decimal requiredQty,
        int pickSequence)
    {
        var line = PickListLine.Create(
            PickListId, shipmentLineId, productCode, lotNumber, locationId, binId, requiredQty, pickSequence);
        _lines.Add(line);
        return line;
    }

    public void StartPicking(string? pickedBy)
    {
        if (Status != PickListStatus.Pending)
            throw new DomainException($"Phiếu lấy hàng #{PickListId} phải ở trạng thái Chờ để bắt đầu.");
        Status = PickListStatus.InProgress;
        Touch(pickedBy);
    }

    public void ConfirmLine(long pickLineId, decimal pickedQty)
    {
        if (Status == PickListStatus.Completed)
            throw new DomainException("Phiếu lấy hàng đã hoàn thành.");
        var line = _lines.FirstOrDefault(l => l.PickLineId == pickLineId)
            ?? throw new DomainException($"Không tìm thấy dòng lấy hàng #{pickLineId}.");
        line.Confirm(pickedQty);
        if (Status == PickListStatus.Pending)
            Status = PickListStatus.InProgress;
    }

    public void Complete(string? completedBy)
    {
        if (Status == PickListStatus.Completed)
            throw new DomainException("Phiếu lấy hàng đã hoàn thành.");
        if (_lines.Count == 0)
            throw new DomainException("Phiếu lấy hàng không có dòng nào.");
        if (_lines.Any(l => !l.IsConfirmed))
            throw new DomainException("Còn dòng lấy hàng chưa được xác nhận. Vui lòng xác nhận tất cả trước khi hoàn thành.");
        Status = PickListStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Touch(completedBy);
    }
}
