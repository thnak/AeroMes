using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.StorageLocations.Commands.DeleteStorageLocation;

public class DeleteStorageLocationHandler(
    IStorageLocationRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteStorageLocationCommand>
{
    public async Task HandleAsync(DeleteStorageLocationCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new EntityNotFoundException("StorageLocation", cmd.Id);
        entity.Deactivate();
        await uow.SaveChangesAsync(ct);
    }
}
