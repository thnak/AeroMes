using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.SubstituteMaterials.Commands.UpdateSubstituteMaterial;

public class UpdateSubstituteMaterialHandler(
    ISubstituteMaterialRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateSubstituteMaterialCommand> validator)
    : ICommandHandler<UpdateSubstituteMaterialCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateSubstituteMaterialCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByIdAsync(cmd.SubstituteId, ct);
            if (entity is null)
                return ValidationResult<Unit>.NotFound($"Substitute material {cmd.SubstituteId} not found.");

            entity.Update(
                cmd.ConversionRatio,
                cmd.Priority,
                cmd.Notes,
                cmd.EffectiveDate,
                cmd.ExpiryDate,
                cmd.UpdatedBy);

            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
