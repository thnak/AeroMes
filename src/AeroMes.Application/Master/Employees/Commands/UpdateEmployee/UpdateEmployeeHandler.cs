using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Employees.Commands.UpdateEmployee;

public class UpdateEmployeeHandler(
    IEmployeeRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateEmployeeCommand>
{
    public async Task HandleAsync(UpdateEmployeeCommand cmd, CancellationToken ct)
    {
        var employee = await repo.GetByIdAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.Code), cmd.Code);
        employee.UpdateDetails(
            cmd.FullName, cmd.Department,
            cmd.RoleType, cmd.DefaultWorkCenterId,
            cmd.IsActive, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
