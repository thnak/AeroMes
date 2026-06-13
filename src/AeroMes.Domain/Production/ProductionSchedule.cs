using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Production;

public enum ScheduleStatus { Draft, InProgress, Completed, Approved }

public class ProductionSchedule : AuditableEntity
{
    public int ScheduleId { get; private set; }
    public string ScheduleName { get; private set; } = string.Empty;
    public string? FacilityCode { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public ScheduleStatus Status { get; private set; } = ScheduleStatus.Draft;

    private readonly List<ProductionScheduleLine> _lines = [];
    public IReadOnlyList<ProductionScheduleLine> Lines => _lines.AsReadOnly();

    private ProductionSchedule() { }

    public static ProductionSchedule Create(
        string scheduleName, string? facilityCode,
        DateTime periodStart, DateTime periodEnd, string? createdBy)
    {
        if (periodEnd <= periodStart)
            throw new DomainException("PeriodEnd must be after PeriodStart.");
        return new ProductionSchedule
        {
            ScheduleName = scheduleName.Trim(),
            FacilityCode = facilityCode?.Trim(),
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Status = ScheduleStatus.Draft,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetLines(IReadOnlyList<ProductionScheduleLine> lines)
    {
        if (Status is ScheduleStatus.Approved)
            throw new DomainException("Cannot modify an approved schedule.");
        _lines.Clear();
        _lines.AddRange(lines);
    }

    public void Complete(string updatedBy)
    {
        if (Status is ScheduleStatus.Approved or ScheduleStatus.Completed)
            throw new DomainException($"Schedule is already {Status}.");
        Status = ScheduleStatus.Completed;
        Touch(updatedBy);
    }

    public void SaveAsDraft(string updatedBy)
    {
        if (Status is ScheduleStatus.Approved)
            throw new DomainException("Cannot revert an approved schedule to Draft.");
        Status = ScheduleStatus.Draft;
        Touch(updatedBy);
    }
}

public class ProductionScheduleLine
{
    public int LineId { get; private set; }
    public int ScheduleId { get; private set; }
    public int POID { get; private set; }
    public int WorkCenterID { get; private set; }
    public DateTime PlannedStart { get; private set; }
    public DateTime PlannedEnd { get; private set; }
    public int SequenceNo { get; private set; }
    public string? Notes { get; private set; }

    private ProductionScheduleLine() { }

    public static ProductionScheduleLine Create(
        int scheduleId, int poid, int workCenterId,
        DateTime plannedStart, DateTime plannedEnd, int sequenceNo, string? notes = null)
    {
        if (plannedEnd <= plannedStart)
            throw new DomainException("PlannedEnd must be after PlannedStart.");
        return new ProductionScheduleLine
        {
            ScheduleId = scheduleId,
            POID = poid,
            WorkCenterID = workCenterId,
            PlannedStart = plannedStart,
            PlannedEnd = plannedEnd,
            SequenceNo = sequenceNo,
            Notes = notes
        };
    }
}
