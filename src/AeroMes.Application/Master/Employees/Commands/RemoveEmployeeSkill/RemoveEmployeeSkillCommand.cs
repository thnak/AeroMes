using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Employees.Commands.RemoveEmployeeSkill;

public record RemoveEmployeeSkillCommand(
    string EmployeeCode,
    int EmployeeSkillId,
    string? DeletedBy) : ICommand<ValidationResult<Unit>>;
