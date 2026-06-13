using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.StorageLocations.Commands.UpdateStorageLocation;

public class UpdateStorageLocationHandler(
    IStorageLocationRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateStorageLocationCommand> validator) : ICommandHandler<UpdateStorageLocationCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateStorageLocationCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByIdAsync(cmd.Id, ct);
            if (entity is null) return ValidationResult<Unit>.NotFound($"StorageLocation '{cmd.Id}' was not found.");
            entity.UpdateDetails(cmd.Name, cmd.LocationType, cmd.WorkCenterId);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
