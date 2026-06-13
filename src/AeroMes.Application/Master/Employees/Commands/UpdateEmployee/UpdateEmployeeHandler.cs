using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Employees.Commands.UpdateEmployee;

public class UpdateEmployeeHandler(
    IEmployeeRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateEmployeeCommand> validator) : ICommandHandler<UpdateEmployeeCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateEmployeeCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var employee = await repo.GetByIdAsync(cmd.Code, ct)
                ?? throw new EntityNotFoundException(nameof(cmd.Code), cmd.Code);
            employee.UpdateDetails(
                cmd.FullName, cmd.Department,
                cmd.RoleType, cmd.DefaultWorkCenterId,
                cmd.IsActive, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<Unit>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
