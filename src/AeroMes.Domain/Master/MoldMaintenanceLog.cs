using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class MoldMaintenanceLog : Entity
{
    public long LogId { get; private set; }
    public int MoldId { get; private set; }
    public MoldMaintenanceType MaintenanceType { get; private set; }
    public long ShotsAtEvent { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public string? TechnicianId { get; private set; }
    public string? Description { get; private set; }
    public string? PartReplaced { get; private set; }
    public decimal? Cost { get; private set; }
    public long? NextDueShots { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private MoldMaintenanceLog() { }

    internal static MoldMaintenanceLog Create(
        int moldId, MoldMaintenanceType maintenanceType, long shotsAtEvent,
        DateTime startDate, DateTime? endDate,
        string? technicianId, string? description, string? partReplaced,
        decimal? cost, long? nextDueShots)
    {
        return new MoldMaintenanceLog
        {
            MoldId = moldId,
            MaintenanceType = maintenanceType,
            ShotsAtEvent = shotsAtEvent,
            StartDate = startDate,
            EndDate = endDate,
            TechnicianId = technicianId?.Trim(),
            Description = description,
            PartReplaced = partReplaced,
            Cost = cost,
            NextDueShots = nextDueShots,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
