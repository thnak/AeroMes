using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Employees.Commands.RemoveShiftAssignment;

public record RemoveShiftAssignmentCommand(
    string EmployeeCode,
    int ShiftAssignmentId,
    string? DeletedBy) : ICommand;
