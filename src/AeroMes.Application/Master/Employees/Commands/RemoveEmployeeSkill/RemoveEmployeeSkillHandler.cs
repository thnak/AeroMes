using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Employees.Commands.RemoveEmployeeSkill;

public class RemoveEmployeeSkillHandler(
    IEmployeeRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveEmployeeSkillCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(RemoveEmployeeSkillCommand cmd, CancellationToken ct)
    {
        var employee = await repo.GetByIdWithDetailsAsync(cmd.EmployeeCode, ct);
        if (employee is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.EmployeeCode}' was not found.");
        employee.RemoveSkill(cmd.EmployeeSkillId);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
