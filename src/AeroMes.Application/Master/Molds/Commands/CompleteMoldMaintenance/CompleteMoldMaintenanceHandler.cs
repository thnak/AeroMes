using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.CompleteMoldMaintenance;

public class CompleteMoldMaintenanceHandler(
    IMoldRepository repo,
    IUnitOfWork uow) : ICommandHandler<CompleteMoldMaintenanceCommand, long>
{
    public async Task<long> HandleAsync(CompleteMoldMaintenanceCommand cmd, CancellationToken ct)
    {
        var mold = await repo.GetByCodeAsync(cmd.MoldCode, ct)
            ?? throw new EntityNotFoundException(nameof(Mold), cmd.MoldCode);

        var log = mold.CompleteMaintenance(
            cmd.MaintenanceType, cmd.StartDate, cmd.EndDate,
            cmd.TechnicianId, cmd.Description, cmd.PartReplaced,
            cmd.Cost, cmd.NextDueShots, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return log.LogId;
    }
}
