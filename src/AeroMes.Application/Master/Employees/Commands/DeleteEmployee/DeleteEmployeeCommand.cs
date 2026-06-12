using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Employees.Commands.DeleteEmployee;

public record DeleteEmployeeCommand(string Code, string? DeletedBy) : ICommand;
