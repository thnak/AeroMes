using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Employees.Commands.EndShiftAssignment;

public class EndShiftAssignmentHandler(
    IEmployeeRepository repo,
    IUnitOfWork uow) : ICommandHandler<EndShiftAssignmentCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(EndShiftAssignmentCommand cmd, CancellationToken ct)
    {
        var employee = await repo.GetByIdWithDetailsAsync(cmd.EmployeeCode, ct);
        if (employee is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.EmployeeCode}' was not found.");
        employee.EndShiftAssignment(cmd.ShiftAssignmentId, cmd.ValidTo);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
