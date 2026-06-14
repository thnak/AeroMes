using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.AlertThresholds.Commands.UpdateAlertThreshold;

public class UpdateAlertThresholdHandler(
    IAlertThresholdRepository repo,
    IMachineRepository machineRepo,
    IWorkCenterRepository workCenterRepo,
    IUnitOfWork uow,
    IValidator<UpdateAlertThresholdCommand> validator) : ICommandHandler<UpdateAlertThresholdCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateAlertThresholdCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        var entity = await repo.GetByIdAsync(cmd.ThresholdId, ct);
        if (entity is null)
            return ValidationResult<Unit>.NotFound($"AlertThreshold {cmd.ThresholdId} không tồn tại.");

        if (cmd.Scope == AlertScope.Machine && cmd.ScopeId != null)
        {
            if (!await machineRepo.ExistsAsync(cmd.ScopeId, ct))
                return ValidationResult<Unit>.NotFound($"Máy '{cmd.ScopeId}' không tồn tại hoặc đã bị xóa.");
        }
        else if (cmd.Scope == AlertScope.WorkCenter && cmd.ScopeId != null)
        {
            if (!await workCenterRepo.CodeExistsAsync(cmd.ScopeId, ct))
                return ValidationResult<Unit>.NotFound($"Work center '{cmd.ScopeId}' không tồn tại hoặc đã bị xóa.");
        }

        try
        {
            entity.UpdateDetails(cmd.MetricKey, cmd.Scope, cmd.ScopeId,
                cmd.WarningLevel, cmd.CriticalLevel, cmd.IsActive,
                cmd.CooldownMinutes, cmd.EmailEnabled, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
