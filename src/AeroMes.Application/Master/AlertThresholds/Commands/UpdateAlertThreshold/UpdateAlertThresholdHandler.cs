using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.AlertThresholds.Commands.UpdateAlertThreshold;

public class UpdateAlertThresholdHandler(
    IAlertThresholdRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateAlertThresholdCommand> validator) : ICommandHandler<UpdateAlertThresholdCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateAlertThresholdCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByIdAsync(cmd.ThresholdId, ct)
                ?? throw new EntityNotFoundException("AlertThreshold", cmd.ThresholdId);

            entity.UpdateDetails(cmd.MetricKey, cmd.Scope, cmd.ScopeId,
                cmd.WarningLevel, cmd.CriticalLevel, cmd.IsActive, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<Unit>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
