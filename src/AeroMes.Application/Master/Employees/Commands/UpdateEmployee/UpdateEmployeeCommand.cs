using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Employees.Commands.UpdateEmployee;

public record UpdateEmployeeCommand(
    string Code,
    string FullName,
    string? Department,
    EmployeeRoleType RoleType,
    int? DefaultWorkCenterId,
    bool IsActive,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
