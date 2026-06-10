using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class WorkCalendar : AuditableEntity
{
    public int WorkCalendarId { get; private set; }
    public string CalendarCode { get; private set; } = string.Empty;
    public string CalendarName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<CalendarDay> _days = [];
    public IReadOnlyList<CalendarDay> Days => _days.AsReadOnly();

    private readonly List<CalendarException> _exceptions = [];
    public IReadOnlyList<CalendarException> Exceptions => _exceptions.AsReadOnly();

    private WorkCalendar() { }

    public static WorkCalendar Create(
        string code, string name, string? description,
        IEnumerable<(DayOfWeek Day, bool IsWorking, IEnumerable<(int ShiftId, int Seq)> Shifts)> days,
        string? createdBy)
    {
        var cal = new WorkCalendar
        {
            CalendarCode = code.Trim().ToUpperInvariant(),
            CalendarName = name.Trim(),
            Description = description?.Trim(),
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
        foreach (var (dow, isWorking, shifts) in days)
            cal._days.Add(CalendarDay.Create(dow, isWorking, shifts));
        return cal;
    }

    public void UpdateDetails(
        string name, string? description, bool isActive,
        IEnumerable<(DayOfWeek Day, bool IsWorking, IEnumerable<(int ShiftId, int Seq)> Shifts)> days,
        string? updatedBy)
    {
        CalendarName = name.Trim();
        Description = description?.Trim();
        IsActive = isActive;
        _days.Clear();
        foreach (var (dow, isWorking, shifts) in days)
            _days.Add(CalendarDay.Create(dow, isWorking, shifts));
        Touch(updatedBy);
    }

    public CalendarException AddException(DateOnly date, CalendarExceptionType type, int? workShiftId, string? createdBy)
    {
        var ex = CalendarException.Create(date, type, workShiftId, createdBy);
        _exceptions.Add(ex);
        return ex;
    }

    public void RemoveException(int exceptionId) =>
        _exceptions.RemoveAll(x => x.CalendarExceptionId == exceptionId);
}
