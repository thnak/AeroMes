using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Operations.Commands.UpdateOperation;

public class UpdateOperationHandler(
    IOperationRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateOperationCommand> validator) : ICommandHandler<UpdateOperationCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateOperationCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByCodeAsync(cmd.Code, ct);
            if (entity is null) return ValidationResult<Unit>.NotFound($"Operation '{cmd.Code}' was not found.");
            entity.UpdateDetails(cmd.Name, cmd.Description);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
