using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class CalendarShift : Entity
{
    public int CalendarShiftId { get; private set; }
    public int CalendarDayId { get; private set; }
    public int WorkShiftId { get; private set; }
    public int Sequence { get; private set; }

    public WorkShift? WorkShift { get; private set; }

    private CalendarShift() { }

    internal static CalendarShift Create(int workShiftId, int sequence) =>
        new() { WorkShiftId = workShiftId, Sequence = sequence };
}
