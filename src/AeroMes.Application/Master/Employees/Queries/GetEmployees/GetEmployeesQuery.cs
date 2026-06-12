using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Employees.Queries.GetEmployees;

public record GetEmployeesQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<EmployeeDto>>;

public record EmployeeDto(
    string EmployeeCode,
    string FullName,
    string? Department,
    string RoleType,
    int? DefaultWorkCenterId,
    string? DefaultWorkCenterName,
    bool IsActive,
    int SkillCount);
