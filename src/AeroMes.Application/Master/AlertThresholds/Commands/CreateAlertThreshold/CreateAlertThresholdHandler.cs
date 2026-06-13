using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.AlertThresholds.Commands.CreateAlertThreshold;

public class CreateAlertThresholdHandler(
    IAlertThresholdRepository repo,
    IUnitOfWork uow,
    IValidator<CreateAlertThresholdCommand> validator) : ICommandHandler<CreateAlertThresholdCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateAlertThresholdCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = AlertThreshold.Create(
                cmd.MetricKey, cmd.Scope, cmd.WarningLevel, cmd.CriticalLevel,
                cmd.ScopeId, cmd.CreatedBy);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.ThresholdId);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<int>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
