using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Employees.Commands.SetEmployeeSkill;

public class SetEmployeeSkillHandler(
    IEmployeeRepository repo,
    IUnitOfWork uow) : ICommandHandler<SetEmployeeSkillCommand, int>
{
    public async Task<int> HandleAsync(SetEmployeeSkillCommand cmd, CancellationToken ct)
    {
        var employee = await repo.GetByIdWithDetailsAsync(cmd.EmployeeCode, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.EmployeeCode), cmd.EmployeeCode);
        var skill = employee.SetSkill(
            cmd.OperationCode, cmd.CertificationLevel,
            cmd.CertifiedAt, cmd.ExpiresAt);
        await uow.SaveChangesAsync(ct);
        return skill.EmployeeSkillId;
    }
}
