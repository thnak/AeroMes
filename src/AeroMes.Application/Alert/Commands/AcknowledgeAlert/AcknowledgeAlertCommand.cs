using AeroMes.Application.Common;
using AeroMes.Domain.Alert;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Alert.Commands.AcknowledgeAlert;

public record AcknowledgeAlertCommand(long AlertEventId, string AcknowledgedBy) : ICommand<ValidationResult<Unit>>;

public class AcknowledgeAlertHandler(IAlertEventRepository repo)
    : ICommandHandler<AcknowledgeAlertCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(AcknowledgeAlertCommand cmd, CancellationToken ct)
    {
        var alert = await repo.GetByIdAsync(cmd.AlertEventId, ct);
        if (alert is null) return ValidationResult<Unit>.NotFound("Alert not found.");
        alert.Acknowledge(cmd.AcknowledgedBy);
        await repo.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
