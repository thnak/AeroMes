using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Employees.Commands.AddShiftAssignment;

public record AddShiftAssignmentCommand(
    string EmployeeCode,
    int WorkCenterId,
    string ShiftCode,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    string? CreatedBy) : ICommand<int>;
