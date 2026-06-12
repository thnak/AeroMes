using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.RegisterMold;

public class RegisterMoldHandler(
    IMoldRepository repo,
    IUnitOfWork uow) : ICommandHandler<RegisterMoldCommand, string>
{
    public async Task<string> HandleAsync(RegisterMoldCommand cmd, CancellationToken ct)
    {
        var mold = Mold.Create(
            cmd.Code, cmd.Name, cmd.MoldType,
            cmd.Material, cmd.Cavities, cmd.MaxShots, cmd.PmIntervalShots,
            cmd.Manufacturer, cmd.PurchaseDate, cmd.PurchaseCost, cmd.WeightKg,
            cmd.StorageLocation, cmd.Notes, cmd.CreatedBy);
        await repo.AddAsync(mold, ct);
        await uow.SaveChangesAsync(ct);
        return mold.MoldCode;
    }
}
