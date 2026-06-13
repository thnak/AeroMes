using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.AcknowledgeReplenishmentAlert;

public class AcknowledgeReplenishmentAlertHandler(
    IReplenishmentAlertRepository repo,
    IUnitOfWork uow)
    : ICommandHandler<AcknowledgeReplenishmentAlertCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        AcknowledgeReplenishmentAlertCommand cmd, CancellationToken ct)
    {
        try
        {
            var alert = await repo.GetByIdAsync(cmd.AlertId, ct);
            if (alert is null)
                return ValidationResult<Unit>.NotFound($"Không tìm thấy cảnh báo #{cmd.AlertId}.");

            alert.Acknowledge(cmd.AcknowledgedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
