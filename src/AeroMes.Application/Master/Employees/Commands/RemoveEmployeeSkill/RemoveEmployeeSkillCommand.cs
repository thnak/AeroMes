using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Employees.Commands.RemoveEmployeeSkill;

public record RemoveEmployeeSkillCommand(
    string EmployeeCode,
    int EmployeeSkillId,
    string? DeletedBy) : ICommand;
