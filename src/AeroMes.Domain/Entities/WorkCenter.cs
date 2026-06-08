using AeroMes.Domain.Common;

namespace AeroMes.Domain.Entities;

public class WorkCenter : AuditableEntity
{
    public int WorkCenterID { get; set; }
    public string WorkCenterCode { get; set; } = string.Empty;
    public string WorkCenterName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<WorkOrder> WorkOrders { get; set; } = [];
    public ICollection<DowntimeLog> DowntimeLogs { get; set; } = [];
}
