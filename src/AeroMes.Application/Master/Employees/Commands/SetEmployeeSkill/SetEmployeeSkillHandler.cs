using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Employees.Commands.SetEmployeeSkill;

public class SetEmployeeSkillHandler(
    IEmployeeRepository repo,
    IUnitOfWork uow,
    IValidator<SetEmployeeSkillCommand> validator) : ICommandHandler<SetEmployeeSkillCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(SetEmployeeSkillCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var employee = await repo.GetByIdWithDetailsAsync(cmd.EmployeeCode, ct);
            if (employee is null) return ValidationResult<int>.NotFound($"Entity '{cmd.EmployeeCode}' was not found.");
            var skill = employee.SetSkill(
                cmd.OperationCode, cmd.CertificationLevel,
                cmd.CertifiedAt, cmd.ExpiresAt);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(skill.EmployeeSkillId);
        }        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
