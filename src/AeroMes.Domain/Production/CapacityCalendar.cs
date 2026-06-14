using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production;

public class CapacityCalendar : Entity
{
    public int WorkCenterID { get; private set; }
    public DateOnly CalendarDate { get; private set; }
    public int ShiftTemplateId { get; private set; }
    public int AvailableMinutes { get; private set; }
    public bool IsWorkingDay { get; private set; } = true;
    public string? Notes { get; private set; }

    // EF navigation
    public Master.WorkCenter? WorkCenter { get; private set; }

    private CapacityCalendar() { }

    public static CapacityCalendar Create(
        int workCenterId, DateOnly calendarDate, int shiftTemplateId,
        int availableMinutes, bool isWorkingDay = true, string? notes = null)
    {
        return new CapacityCalendar
        {
            WorkCenterID = workCenterId,
            CalendarDate = calendarDate,
            ShiftTemplateId = shiftTemplateId,
            AvailableMinutes = availableMinutes,
            IsWorkingDay = isWorkingDay,
            Notes = notes
        };
    }

    public void Update(int availableMinutes, bool isWorkingDay, string? notes)
    {
        AvailableMinutes = availableMinutes;
        IsWorkingDay = isWorkingDay;
        Notes = notes;
    }
}
