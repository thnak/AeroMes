using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class WorkShift : AuditableEntity
{
    public int WorkShiftId { get; private set; }
    public string ShiftCode { get; private set; } = string.Empty;
    public string ShiftName { get; private set; } = string.Empty;
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public bool IsNightShift { get; private set; }
    public int NetMinutes { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<ShiftBreak> _breaks = [];
    public IReadOnlyList<ShiftBreak> Breaks => _breaks.AsReadOnly();

    private WorkShift() { }

    public static WorkShift Create(
        string code, string name,
        TimeOnly startTime, TimeOnly endTime,
        IEnumerable<(TimeOnly Start, TimeOnly End)> breaks,
        string? createdBy)
    {
        var shift = new WorkShift
        {
            ShiftCode = code.Trim().ToUpperInvariant(),
            ShiftName = name.Trim(),
            StartTime = startTime,
            EndTime = endTime,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
        foreach (var (s, e) in breaks)
            shift._breaks.Add(ShiftBreak.Create(s, e));
        shift.RecalcNet();
        return shift;
    }

    public void UpdateDetails(
        string name, TimeOnly startTime, TimeOnly endTime,
        IEnumerable<(TimeOnly Start, TimeOnly End)> breaks,
        bool isActive, string? updatedBy)
    {
        ShiftName = name.Trim();
        StartTime = startTime;
        EndTime = endTime;
        IsActive = isActive;
        _breaks.Clear();
        foreach (var (s, e) in breaks)
            _breaks.Add(ShiftBreak.Create(s, e));
        RecalcNet();
        Touch(updatedBy);
    }

    private void RecalcNet()
    {
        var durationMinutes = EndTime > StartTime
            ? (int)(EndTime - StartTime).TotalMinutes
            : (int)(TimeSpan.FromHours(24) - (StartTime - EndTime)).TotalMinutes;
        IsNightShift = EndTime < StartTime;
        var breakTotal = _breaks.Sum(b => b.BreakMinutes);
        NetMinutes = Math.Max(0, durationMinutes - breakTotal);
    }
}
