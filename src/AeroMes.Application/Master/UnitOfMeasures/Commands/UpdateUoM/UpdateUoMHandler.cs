using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.UnitOfMeasures.Commands.UpdateUoM;

public class UpdateUoMHandler(
    IUnitOfMeasureRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateUoMCommand> validator) : ICommandHandler<UpdateUoMCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateUoMCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByCodeAsync(cmd.Code, ct);
            if (entity is null) return ValidationResult<Unit>.NotFound($"UnitOfMeasure '{cmd.Code}' was not found.");
            entity.Update(cmd.Name, cmd.Group);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
