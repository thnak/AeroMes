using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.UpdateMold;

public class UpdateMoldHandler(
    IMoldRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateMoldCommand>
{
    public async Task HandleAsync(UpdateMoldCommand cmd, CancellationToken ct)
    {
        var mold = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException(nameof(Mold), cmd.Code);

        mold.UpdateDetails(
            cmd.Name, cmd.MoldType,
            cmd.Material, cmd.Cavities, cmd.MaxShots, cmd.PmIntervalShots,
            cmd.Manufacturer, cmd.PurchaseDate, cmd.PurchaseCost, cmd.WeightKg,
            cmd.StorageLocation, cmd.Notes, cmd.IsActive, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
