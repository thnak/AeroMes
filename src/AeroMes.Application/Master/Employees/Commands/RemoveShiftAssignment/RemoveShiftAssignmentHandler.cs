using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Employees.Commands.RemoveShiftAssignment;

public class RemoveShiftAssignmentHandler(
    IEmployeeRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveShiftAssignmentCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(RemoveShiftAssignmentCommand cmd, CancellationToken ct)
    {
        var employee = await repo.GetByIdWithDetailsAsync(cmd.EmployeeCode, ct);
        if (employee is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.EmployeeCode}' was not found.");
        employee.RemoveShiftAssignment(cmd.ShiftAssignmentId);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
