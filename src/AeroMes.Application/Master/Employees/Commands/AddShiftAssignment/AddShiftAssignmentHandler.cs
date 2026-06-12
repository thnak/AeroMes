using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Employees.Commands.AddShiftAssignment;

public class AddShiftAssignmentHandler(
    IEmployeeRepository repo,
    IUnitOfWork uow) : ICommandHandler<AddShiftAssignmentCommand, int>
{
    public async Task<int> HandleAsync(AddShiftAssignmentCommand cmd, CancellationToken ct)
    {
        var employee = await repo.GetByIdWithDetailsAsync(cmd.EmployeeCode, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.EmployeeCode), cmd.EmployeeCode);
        var assignment = employee.AddShiftAssignment(
            cmd.WorkCenterId, cmd.ShiftCode,
            cmd.ValidFrom, cmd.ValidTo);
        await uow.SaveChangesAsync(ct);
        return assignment.ShiftAssignmentId;
    }
}
