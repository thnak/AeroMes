using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.AlertThresholds.Commands.UpdateAlertThreshold;

public class UpdateAlertThresholdHandler(
    IAlertThresholdRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateAlertThresholdCommand>
{
    public async Task HandleAsync(UpdateAlertThresholdCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.ThresholdId, ct)
            ?? throw new EntityNotFoundException("AlertThreshold", cmd.ThresholdId);

        entity.UpdateDetails(cmd.MetricKey, cmd.Scope, cmd.ScopeId,
            cmd.WarningLevel, cmd.CriticalLevel, cmd.IsActive, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
