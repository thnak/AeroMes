using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class CalendarException : Entity
{
    public int CalendarExceptionId { get; private set; }
    public int WorkCalendarId { get; private set; }
    public DateOnly Date { get; private set; }
    public CalendarExceptionType ExceptionType { get; private set; }
    public int? WorkShiftId { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public WorkShift? WorkShift { get; private set; }

    private CalendarException() { }

    internal static CalendarException Create(
        DateOnly date, CalendarExceptionType type, int? workShiftId, string? createdBy) =>
        new()
        {
            Date = date,
            ExceptionType = type,
            WorkShiftId = workShiftId,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
}
