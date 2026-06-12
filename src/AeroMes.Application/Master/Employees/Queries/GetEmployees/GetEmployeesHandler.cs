using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Employees.Queries.GetEmployees;

public class GetEmployeesHandler(IEmployeeRepository repo)
    : IQueryHandler<GetEmployeesQuery, IReadOnlyList<EmployeeDto>>
{
    public async Task<IReadOnlyList<EmployeeDto>> HandleAsync(GetEmployeesQuery query, CancellationToken ct)
    {
        var employees = await repo.GetAllAsync(query.ActiveOnly, ct);
        return employees
            .Select(e => new EmployeeDto(
                e.EmployeeCode, e.FullName, e.Department,
                e.RoleType.ToString(),
                e.DefaultWorkCenterId, e.DefaultWorkCenter?.WorkCenterName,
                e.IsActive, e.Skills.Count))
            .ToList();
    }
}
