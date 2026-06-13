using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.AlertThresholds.Commands.DeleteAlertThreshold;

public class DeleteAlertThresholdHandler(
    IAlertThresholdRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteAlertThresholdCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteAlertThresholdCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.ThresholdId, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"AlertThreshold '{cmd.ThresholdId}' was not found.");

        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
