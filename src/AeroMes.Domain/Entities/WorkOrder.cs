using AeroMes.Domain.Common;

namespace AeroMes.Domain.Entities;

public class WorkOrder : AuditableEntity
{
    public int WorkOrderID { get; set; }
    public string WorkOrderNo { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int TargetQuantity { get; set; }
    public int ActualQtyOK { get; set; }
    public int ActualQtyNG { get; set; }
    public int WorkCenterID { get; set; }
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Released;
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }

    public WorkCenter WorkCenter { get; set; } = null!;
    public ICollection<ProductionLog> ProductionLogs { get; set; } = [];
    public ICollection<DowntimeLog> DowntimeLogs { get; set; } = [];
}

public enum WorkOrderStatus
{
    Released,
    Running,
    Paused,
    Completed,
    Cancelled
}
