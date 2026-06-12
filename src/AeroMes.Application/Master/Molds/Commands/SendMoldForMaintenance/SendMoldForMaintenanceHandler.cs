using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.SendMoldForMaintenance;

public class SendMoldForMaintenanceHandler(
    IMoldRepository repo,
    IUnitOfWork uow) : ICommandHandler<SendMoldForMaintenanceCommand>
{
    public async Task HandleAsync(SendMoldForMaintenanceCommand cmd, CancellationToken ct)
    {
        var mold = await repo.GetByCodeAsync(cmd.MoldCode, ct)
            ?? throw new EntityNotFoundException(nameof(Mold), cmd.MoldCode);

        mold.SendForMaintenance(cmd.MaintenanceType, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
