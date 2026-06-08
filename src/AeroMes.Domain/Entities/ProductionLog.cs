namespace AeroMes.Domain.Entities;

public class ProductionLog
{
    public long LogID { get; set; }
    public int WorkOrderID { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int QtyOK { get; set; }
    public int QtyNG { get; set; }
    public string OperatorID { get; set; } = string.Empty;
    public string? MachineCode { get; set; }
    public string? ShiftCode { get; set; }
    public string? Notes { get; set; }
    public string? IdempotencyKey { get; set; }

    public WorkOrder WorkOrder { get; set; } = null!;
    public ICollection<DefectDetail> DefectDetails { get; set; } = [];
}
