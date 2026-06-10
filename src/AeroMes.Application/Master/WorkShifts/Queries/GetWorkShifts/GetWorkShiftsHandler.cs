using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.WorkShifts.Queries.GetWorkShifts;

public class GetWorkShiftsHandler(IWorkShiftRepository repo)
    : IQueryHandler<GetWorkShiftsQuery, IReadOnlyList<WorkShiftDto>>
{
    public async Task<IReadOnlyList<WorkShiftDto>> HandleAsync(GetWorkShiftsQuery query, CancellationToken ct)
    {
        var list = await repo.GetAllAsync(query.ActiveOnly, ct);
        return list.Select(x => new WorkShiftDto(
            x.WorkShiftId, x.ShiftCode, x.ShiftName,
            x.StartTime, x.EndTime, x.IsNightShift, x.NetMinutes, x.IsActive)).ToList();
    }
}
