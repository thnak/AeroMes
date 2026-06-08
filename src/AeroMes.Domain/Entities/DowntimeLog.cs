namespace AeroMes.Domain.Entities;

public class DowntimeLog
{
    public long DowntimeLogID { get; set; }
    public int WorkCenterID { get; set; }
    public string MachineCode { get; set; } = string.Empty;
    public string ReasonCode { get; set; } = string.Empty;
    public string? ReasonName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? OperatorID { get; set; }
    public string? Notes { get; set; }

    public int? DurationMinutes =>
        EndTime.HasValue ? (int)(EndTime.Value - StartTime).TotalMinutes : null;

    public WorkCenter WorkCenter { get; set; } = null!;
}
