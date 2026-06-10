using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.AlertThresholds.Commands.CreateAlertThreshold;

public class CreateAlertThresholdHandler(
    IAlertThresholdRepository repo,
    IUnitOfWork uow) : ICommandHandler<CreateAlertThresholdCommand, int>
{
    public async Task<int> HandleAsync(CreateAlertThresholdCommand cmd, CancellationToken ct)
    {
        var entity = AlertThreshold.Create(
            cmd.MetricKey, cmd.Scope, cmd.WarningLevel, cmd.CriticalLevel,
            cmd.ScopeId, cmd.CreatedBy);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.ThresholdId;
    }
}
