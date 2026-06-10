using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.WorkShifts.Queries.GetWorkShifts;

public record GetWorkShiftsQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<WorkShiftDto>>;

public record WorkShiftDto(
    int WorkShiftId,
    string ShiftCode,
    string ShiftName,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsNightShift,
    int NetMinutes,
    bool IsActive);
