using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Employees.Commands.DeleteEmployee;

public record DeleteEmployeeCommand(string Code, string? DeletedBy) : ICommand<ValidationResult<Unit>>;
