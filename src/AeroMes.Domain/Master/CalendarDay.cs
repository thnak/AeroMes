using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class CalendarDay : Entity
{
    public int CalendarDayId { get; private set; }
    public int WorkCalendarId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public bool IsWorkingDay { get; private set; }

    private readonly List<CalendarShift> _shifts = [];
    public IReadOnlyList<CalendarShift> Shifts => _shifts.AsReadOnly();

    private CalendarDay() { }

    internal static CalendarDay Create(
        DayOfWeek dow, bool isWorking,
        IEnumerable<(int ShiftId, int Seq)> shifts)
    {
        var day = new CalendarDay { DayOfWeek = dow, IsWorkingDay = isWorking };
        foreach (var (shiftId, seq) in shifts)
            day._shifts.Add(CalendarShift.Create(shiftId, seq));
        return day;
    }
}
