using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Employees.Commands.RemoveShiftAssignment;

public class RemoveShiftAssignmentHandler(
    IEmployeeRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveShiftAssignmentCommand>
{
    public async Task HandleAsync(RemoveShiftAssignmentCommand cmd, CancellationToken ct)
    {
        var employee = await repo.GetByIdWithDetailsAsync(cmd.EmployeeCode, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.EmployeeCode), cmd.EmployeeCode);
        employee.RemoveShiftAssignment(cmd.ShiftAssignmentId);
        await uow.SaveChangesAsync(ct);
    }
}
