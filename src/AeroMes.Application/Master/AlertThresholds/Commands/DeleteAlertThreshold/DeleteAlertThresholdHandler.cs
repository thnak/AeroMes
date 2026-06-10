using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.AlertThresholds.Commands.DeleteAlertThreshold;

public class DeleteAlertThresholdHandler(
    IAlertThresholdRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteAlertThresholdCommand>
{
    public async Task HandleAsync(DeleteAlertThresholdCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.ThresholdId, ct)
            ?? throw new EntityNotFoundException("AlertThreshold", cmd.ThresholdId);

        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
    }
}
