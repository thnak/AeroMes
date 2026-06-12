using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Employees.Queries.GetEmployeeById;

public class GetEmployeeByIdHandler(IEmployeeRepository repo)
    : IQueryHandler<GetEmployeeByIdQuery, EmployeeDetailDto?>
{
    public async Task<EmployeeDetailDto?> HandleAsync(GetEmployeeByIdQuery query, CancellationToken ct)
    {
        var e = await repo.GetByIdWithDetailsAsync(query.Code, ct);
        if (e is null) return null;

        var skills = e.Skills
            .OrderBy(x => x.OperationCode)
            .Select(x => new EmployeeSkillDto(
                x.EmployeeSkillId, x.OperationCode,
                x.Operation?.OperationName,
                x.CertificationLevel, x.CertifiedAt, x.ExpiresAt))
            .ToList();

        var assignments = e.ShiftAssignments
            .OrderBy(x => x.ValidFrom)
            .Select(x => new ShiftAssignmentDto(
                x.ShiftAssignmentId,
                x.WorkCenterId, x.WorkCenter?.WorkCenterName,
                x.ShiftCode, x.ShiftTemplate?.ShiftName,
                x.ShiftTemplate?.StartTime, x.ShiftTemplate?.EndTime,
                x.ValidFrom, x.ValidTo))
            .ToList();

        return new EmployeeDetailDto(
            e.EmployeeCode, e.FullName, e.Department,
            e.RoleType.ToString(),
            e.DefaultWorkCenterId, e.DefaultWorkCenter?.WorkCenterName,
            e.IsActive, skills, assignments);
    }
}
