using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Employees.Commands.CreateEmployee;

public record CreateEmployeeCommand(
    string Code,
    string FullName,
    string? Department,
    EmployeeRoleType RoleType,
    int? DefaultWorkCenterId,
    string? CreatedBy) : ICommand<ValidationResult<string>>;
