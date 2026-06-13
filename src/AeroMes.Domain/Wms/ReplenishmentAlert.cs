using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Wms;

public class ReplenishmentAlert : Entity
{
    public long AlertId { get; private set; }
    public int PolicyId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public int LocationId { get; private set; }
    public decimal CurrentQty { get; private set; }
    public DateTime TriggeredAt { get; private set; }
    public ReplenishmentAlertStatus Status { get; private set; } = ReplenishmentAlertStatus.Open;
    public string? AcknowledgedBy { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }
    public int? LinkedPoId { get; private set; }

    private ReplenishmentAlert() { }

    public static ReplenishmentAlert Create(int policyId, string productCode, int locationId, decimal currentQty)
        => new()
        {
            PolicyId = policyId,
            ProductCode = productCode.Trim().ToUpperInvariant(),
            LocationId = locationId,
            CurrentQty = currentQty,
            TriggeredAt = DateTime.UtcNow,
            Status = ReplenishmentAlertStatus.Open,
        };

    public void Acknowledge(string? acknowledgedBy)
    {
        if (Status != ReplenishmentAlertStatus.Open)
            throw new DomainException("Chỉ có thể xác nhận cảnh báo đang mở.");
        AcknowledgedBy = acknowledgedBy;
        AcknowledgedAt = DateTime.UtcNow;
        Status = ReplenishmentAlertStatus.Acknowledged;
    }

    public void LinkToPurchaseOrder(int poId)
    {
        if (Status is ReplenishmentAlertStatus.Resolved)
            throw new DomainException("Không thể liên kết PO với cảnh báo đã giải quyết.");
        LinkedPoId = poId;
        Status = ReplenishmentAlertStatus.PoCreated;
    }

    public void Resolve()
    {
        if (Status == ReplenishmentAlertStatus.Resolved) return;
        Status = ReplenishmentAlertStatus.Resolved;
    }
}
