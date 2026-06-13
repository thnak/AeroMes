using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Master;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.StorageLocations.Commands.CreateStorageLocation;
public class CreateStorageLocationHandler(
    IStorageLocationRepository repo,
    IUnitOfWork uow,
    IValidator<CreateStorageLocationCommand> validator) : ICommandHandler<CreateStorageLocationCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateStorageLocationCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());
        try
        {
            var entity = StorageLocation.Create(cmd.Code, cmd.Name, cmd.LocationType, cmd.WorkCenterId);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.LocationID);
        }        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
