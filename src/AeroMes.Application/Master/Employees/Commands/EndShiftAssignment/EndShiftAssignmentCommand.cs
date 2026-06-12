using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Employees.Commands.EndShiftAssignment;

public record EndShiftAssignmentCommand(
    string EmployeeCode,
    int ShiftAssignmentId,
    DateOnly ValidTo,
    string? UpdatedBy) : ICommand;
