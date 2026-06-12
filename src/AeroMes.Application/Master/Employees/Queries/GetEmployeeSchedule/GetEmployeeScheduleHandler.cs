using AeroMes.Application.Master.Employees.Queries.GetEmployeeById;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Employees.Queries.GetEmployeeSchedule;

public class GetEmployeeScheduleHandler(IEmployeeRepository repo)
    : IQueryHandler<GetEmployeeScheduleQuery, EmployeeScheduleDto?>
{
    public async Task<EmployeeScheduleDto?> HandleAsync(GetEmployeeScheduleQuery query, CancellationToken ct)
    {
        var e = await repo.GetByIdWithDetailsAsync(query.Code, ct);
        if (e is null) return null;

        var asOf = query.AsOf ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var assignments = e.ShiftAssignments
            .Where(x => x.IsActiveOn(asOf))
            .OrderBy(x => x.ShiftTemplate?.StartTime)
            .Select(x => new ShiftAssignmentDto(
                x.ShiftAssignmentId,
                x.WorkCenterId, x.WorkCenter?.WorkCenterName,
                x.ShiftCode, x.ShiftTemplate?.ShiftName,
                x.ShiftTemplate?.StartTime, x.ShiftTemplate?.EndTime,
                x.ValidFrom, x.ValidTo))
            .ToList();

        return new EmployeeScheduleDto(e.EmployeeCode, e.FullName, asOf, assignments);
    }
}
