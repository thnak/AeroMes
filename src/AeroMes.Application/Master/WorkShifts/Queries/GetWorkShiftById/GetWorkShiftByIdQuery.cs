using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.WorkShifts.Queries.GetWorkShiftById;

public record GetWorkShiftByIdQuery(int WorkShiftId) : IQuery<WorkShiftDetailDto?>;

public record ShiftBreakDto(int ShiftBreakId, TimeOnly BreakStart, TimeOnly BreakEnd, int BreakMinutes);

public record WorkShiftDetailDto(
    int WorkShiftId,
    string ShiftCode,
    string ShiftName,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsNightShift,
    int NetMinutes,
    bool IsActive,
    IReadOnlyList<ShiftBreakDto> Breaks);
