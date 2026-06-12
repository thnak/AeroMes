using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Employees.Commands.RemoveEmployeeSkill;

public class RemoveEmployeeSkillHandler(
    IEmployeeRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveEmployeeSkillCommand>
{
    public async Task HandleAsync(RemoveEmployeeSkillCommand cmd, CancellationToken ct)
    {
        var employee = await repo.GetByIdWithDetailsAsync(cmd.EmployeeCode, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.EmployeeCode), cmd.EmployeeCode);
        employee.RemoveSkill(cmd.EmployeeSkillId);
        await uow.SaveChangesAsync(ct);
    }
}
