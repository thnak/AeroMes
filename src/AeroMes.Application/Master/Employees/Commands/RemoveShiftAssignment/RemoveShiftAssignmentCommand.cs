using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Employees.Commands.RemoveShiftAssignment;

public record RemoveShiftAssignmentCommand(
    string EmployeeCode,
    int ShiftAssignmentId,
    string? DeletedBy) : ICommand<ValidationResult<Unit>>;
