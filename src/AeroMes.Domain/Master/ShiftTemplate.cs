using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

[Flags]
public enum WeekDays
{
    None = 0,
    Mon = 1,
    Tue = 2,
    Wed = 4,
    Thu = 8,
    Fri = 16,
    Sat = 32,
    Sun = 64,
    Weekdays = Mon | Tue | Wed | Thu | Fri,
    All = Mon | Tue | Wed | Thu | Fri | Sat | Sun
}

public class ShiftTemplate : AuditableEntity
{
    public string ShiftCode { get; private set; } = string.Empty;
    public string ShiftName { get; private set; } = string.Empty;
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public bool IsNightShift { get; private set; }
    public WeekDays ValidDays { get; private set; }
    public int? WorkCenterId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public WorkCenter? WorkCenter { get; private set; }

    private ShiftTemplate() { }

    public static ShiftTemplate Create(
        string code, string name,
        TimeOnly startTime, TimeOnly endTime,
        bool isNightShift = false,
        WeekDays validDays = WeekDays.Weekdays,
        int? workCenterId = null,
        string? createdBy = null)
    {
        return new ShiftTemplate
        {
            ShiftCode = code.Trim().ToUpperInvariant(),
            ShiftName = name.Trim(),
            StartTime = startTime,
            EndTime = endTime,
            IsNightShift = isNightShift,
            ValidDays = validDays,
            WorkCenterId = workCenterId,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(
        string name, TimeOnly startTime, TimeOnly endTime,
        bool isNightShift, WeekDays validDays, int? workCenterId,
        bool isActive, string? updatedBy)
    {
        ShiftName = name.Trim();
        StartTime = startTime;
        EndTime = endTime;
        IsNightShift = isNightShift;
        ValidDays = validDays;
        WorkCenterId = workCenterId;
        IsActive = isActive;
        Touch(updatedBy);
    }
}
