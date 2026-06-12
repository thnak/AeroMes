using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.RecordToolMaintenance;

public class RecordToolMaintenanceHandler(
    IToolRepository repo,
    IUnitOfWork uow) : ICommandHandler<RecordToolMaintenanceCommand, long>
{
    public async Task<long> HandleAsync(RecordToolMaintenanceCommand cmd, CancellationToken ct)
    {
        var tool = await repo.GetByCodeAsync(cmd.ToolCode, ct)
            ?? throw new EntityNotFoundException(nameof(Tool), cmd.ToolCode);

        var log = tool.RecordMaintenance(
            cmd.MaintenanceType, cmd.PerformedAt, cmd.PerformedBy,
            cmd.Cost, cmd.NextDueCount, cmd.NextDueDate, cmd.Notes, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return log.LogId;
    }
}
