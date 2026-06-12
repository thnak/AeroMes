using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Employees.Commands.DeleteEmployee;

public class DeleteEmployeeHandler(
    IEmployeeRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteEmployeeCommand>
{
    public async Task HandleAsync(DeleteEmployeeCommand cmd, CancellationToken ct)
    {
        var employee = await repo.GetByIdAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.Code), cmd.Code);
        employee.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
    }
}
