using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Employees.Commands.EndShiftAssignment;

public class EndShiftAssignmentHandler(
    IEmployeeRepository repo,
    IUnitOfWork uow) : ICommandHandler<EndShiftAssignmentCommand>
{
    public async Task HandleAsync(EndShiftAssignmentCommand cmd, CancellationToken ct)
    {
        var employee = await repo.GetByIdWithDetailsAsync(cmd.EmployeeCode, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.EmployeeCode), cmd.EmployeeCode);
        employee.EndShiftAssignment(cmd.ShiftAssignmentId, cmd.ValidTo);
        await uow.SaveChangesAsync(ct);
    }
}
