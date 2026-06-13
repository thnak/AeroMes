using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Maintenance;

public enum MaintenanceOrderType { Preventive, Corrective, Predictive, Calibration }
public enum MaintenanceOrderStatus { Open, InProgress, PendingParts, Completed, Cancelled }
public enum MaintenancePriority { Low, Normal, High, Critical }

public class MaintenanceOrder : AuditableEntity
{
    public int MaintOrderID { get; private set; }
    public string MaintOrderCode { get; private set; } = string.Empty;
    public string MachineCode { get; private set; } = string.Empty;
    public MaintenanceOrderType OrderType { get; private set; }
    public string? TriggerRef { get; private set; }
    public MaintenanceOrderStatus Status { get; private set; } = MaintenanceOrderStatus.Open;
    public MaintenancePriority Priority { get; private set; } = MaintenancePriority.Normal;
    public DateTime? PlannedStartAt { get; private set; }
    public DateTime? PlannedEndAt { get; private set; }
    public DateTime? ActualStartAt { get; private set; }
    public DateTime? ActualEndAt { get; private set; }
    public string? AssignedTo { get; private set; }
    public decimal? EstimatedCost { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<MaintCostLine> _lines = [];
    public IReadOnlyList<MaintCostLine> Lines => _lines.AsReadOnly();
    public decimal ActualTotalCost => _lines.Sum(l => l.LineTotal);

    private MaintenanceOrder() { }

    public static MaintenanceOrder Create(
        string code, string machineCode, MaintenanceOrderType orderType,
        string? triggerRef, MaintenancePriority priority,
        DateTime? plannedStartAt, DateTime? plannedEndAt,
        string? assignedTo, decimal? estimatedCost, string? notes, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Mã lệnh bảo trì không được để trống.");
        if (string.IsNullOrWhiteSpace(machineCode))
            throw new DomainException("Mã máy không được để trống.");

        return new MaintenanceOrder
        {
            MaintOrderCode = code.Trim().ToUpperInvariant(),
            MachineCode = machineCode.Trim().ToUpperInvariant(),
            OrderType = orderType,
            TriggerRef = triggerRef?.Trim(),
            Status = MaintenanceOrderStatus.Open,
            Priority = priority,
            PlannedStartAt = plannedStartAt,
            PlannedEndAt = plannedEndAt,
            AssignedTo = assignedTo?.Trim(),
            EstimatedCost = estimatedCost,
            Notes = notes?.Trim(),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void AddCostLine(MaintCostLine line, string? updatedBy)
    {
        _lines.Add(line);
        Touch(updatedBy);
    }

    public void Start(string? updatedBy)
    {
        if (Status != MaintenanceOrderStatus.Open)
            throw new DomainException($"Lệnh bảo trì phải ở trạng thái Mở. Hiện tại: {Status}.");
        Status = MaintenanceOrderStatus.InProgress;
        ActualStartAt = DateTime.UtcNow;
        Touch(updatedBy);
    }

    public void HoldForParts(string? updatedBy)
    {
        if (Status != MaintenanceOrderStatus.InProgress)
            throw new DomainException($"Chỉ có thể chờ phụ tùng khi đang thực hiện. Hiện tại: {Status}.");
        Status = MaintenanceOrderStatus.PendingParts;
        Touch(updatedBy);
    }

    public void Complete(string? updatedBy)
    {
        if (Status is not (MaintenanceOrderStatus.InProgress or MaintenanceOrderStatus.PendingParts))
            throw new DomainException($"Lệnh bảo trì phải đang thực hiện để hoàn thành. Hiện tại: {Status}.");
        Status = MaintenanceOrderStatus.Completed;
        ActualEndAt = DateTime.UtcNow;
        Touch(updatedBy);
    }

    public void Cancel(string? updatedBy)
    {
        if (Status == MaintenanceOrderStatus.Completed || Status == MaintenanceOrderStatus.Cancelled)
            throw new DomainException($"Không thể hủy lệnh bảo trì ở trạng thái {Status}.");
        Status = MaintenanceOrderStatus.Cancelled;
        Touch(updatedBy);
    }
}
