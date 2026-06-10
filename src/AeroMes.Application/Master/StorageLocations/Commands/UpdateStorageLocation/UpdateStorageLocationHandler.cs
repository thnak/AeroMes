using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.StorageLocations.Commands.UpdateStorageLocation;

public class UpdateStorageLocationHandler(
    IStorageLocationRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateStorageLocationCommand>
{
    public async Task HandleAsync(UpdateStorageLocationCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new EntityNotFoundException("StorageLocation", cmd.Id);
        entity.UpdateDetails(cmd.Name, cmd.LocationType, cmd.WorkCenterId);
        await uow.SaveChangesAsync(ct);
    }
}
