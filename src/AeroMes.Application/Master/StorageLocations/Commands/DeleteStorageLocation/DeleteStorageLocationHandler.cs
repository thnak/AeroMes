using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.StorageLocations.Commands.DeleteStorageLocation;

public class DeleteStorageLocationHandler(
    IStorageLocationRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteStorageLocationCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteStorageLocationCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.Id, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"StorageLocation '{cmd.Id}' was not found.");
        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
