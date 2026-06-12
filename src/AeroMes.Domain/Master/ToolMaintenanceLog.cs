using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class ToolMaintenanceLog : Entity
{
    public long LogId { get; private set; }
    public int ToolId { get; private set; }
    public ToolMaintenanceType MaintenanceType { get; private set; }
    public int UsageAtEvent { get; private set; }
    public DateTime PerformedAt { get; private set; }
    public string? PerformedBy { get; private set; }
    public decimal? Cost { get; private set; }
    public int? NextDueCount { get; private set; }
    public DateOnly? NextDueDate { get; private set; }
    public string? Notes { get; private set; }

    private ToolMaintenanceLog() { }

    internal static ToolMaintenanceLog Create(
        int toolId, ToolMaintenanceType maintenanceType, int usageAtEvent,
        DateTime performedAt, string? performedBy, decimal? cost,
        int? nextDueCount, DateOnly? nextDueDate, string? notes)
    {
        return new ToolMaintenanceLog
        {
            ToolId = toolId,
            MaintenanceType = maintenanceType,
            UsageAtEvent = usageAtEvent,
            PerformedAt = performedAt,
            PerformedBy = performedBy?.Trim(),
            Cost = cost,
            NextDueCount = nextDueCount,
            NextDueDate = nextDueDate,
            Notes = notes,
        };
    }
}
