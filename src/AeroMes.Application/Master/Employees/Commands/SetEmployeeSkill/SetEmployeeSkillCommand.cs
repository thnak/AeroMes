using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Employees.Commands.SetEmployeeSkill;

public record SetEmployeeSkillCommand(
    string EmployeeCode,
    string OperationCode,
    int CertificationLevel,
    DateOnly CertifiedAt,
    DateOnly? ExpiresAt,
    string? UpdatedBy) : ICommand<ValidationResult<int>>;
