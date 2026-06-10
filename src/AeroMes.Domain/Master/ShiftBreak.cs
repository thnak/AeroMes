using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class ShiftBreak : Entity
{
    public int ShiftBreakId { get; private set; }
    public int WorkShiftId { get; private set; }
    public TimeOnly BreakStart { get; private set; }
    public TimeOnly BreakEnd { get; private set; }
    public int BreakMinutes { get; private set; }

    private ShiftBreak() { }

    internal static ShiftBreak Create(TimeOnly start, TimeOnly end)
    {
        var minutes = end > start
            ? (int)(end - start).TotalMinutes
            : (int)(TimeSpan.FromHours(24) - (start - end)).TotalMinutes;
        return new ShiftBreak { BreakStart = start, BreakEnd = end, BreakMinutes = minutes };
    }
}
