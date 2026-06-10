using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.WorkShifts.Queries.GetWorkShiftById;

public class GetWorkShiftByIdHandler(IWorkShiftRepository repo)
    : IQueryHandler<GetWorkShiftByIdQuery, WorkShiftDetailDto?>
{
    public async Task<WorkShiftDetailDto?> HandleAsync(GetWorkShiftByIdQuery query, CancellationToken ct)
    {
        var shift = await repo.GetByIdWithBreaksAsync(query.WorkShiftId, ct);
        if (shift is null) return null;
        return new WorkShiftDetailDto(
            shift.WorkShiftId, shift.ShiftCode, shift.ShiftName,
            shift.StartTime, shift.EndTime, shift.IsNightShift, shift.NetMinutes, shift.IsActive,
            shift.Breaks.Select(b => new ShiftBreakDto(b.ShiftBreakId, b.BreakStart, b.BreakEnd, b.BreakMinutes)).ToList());
    }
}
