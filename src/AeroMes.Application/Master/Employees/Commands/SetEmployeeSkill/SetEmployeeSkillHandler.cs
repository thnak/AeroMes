using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

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
            var employee = await repo.GetByIdWithDetailsAsync(cmd.EmployeeCode, ct)
                ?? throw new EntityNotFoundException(nameof(cmd.EmployeeCode), cmd.EmployeeCode);
            var skill = employee.SetSkill(
                cmd.OperationCode, cmd.CertificationLevel,
                cmd.CertifiedAt, cmd.ExpiresAt);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(skill.EmployeeSkillId);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<int>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
