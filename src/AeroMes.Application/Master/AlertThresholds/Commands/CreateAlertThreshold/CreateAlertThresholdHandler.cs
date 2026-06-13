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
    IMachineRepository machineRepo,
    IWorkCenterRepository workCenterRepo,
    IUnitOfWork uow,
    IValidator<CreateAlertThresholdCommand> validator) : ICommandHandler<CreateAlertThresholdCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateAlertThresholdCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        if (cmd.Scope == AlertScope.Machine && cmd.ScopeId != null)
        {
            if (!await machineRepo.ExistsAsync(cmd.ScopeId, ct))
                return ValidationResult<int>.NotFound($"Máy '{cmd.ScopeId}' không tồn tại hoặc đã bị xóa.");
        }
        else if (cmd.Scope == AlertScope.WorkCenter && cmd.ScopeId != null)
        {
            if (!await workCenterRepo.CodeExistsAsync(cmd.ScopeId, ct))
                return ValidationResult<int>.NotFound($"Work center '{cmd.ScopeId}' không tồn tại hoặc đã bị xóa.");
        }

        try
        {
            var entity = AlertThreshold.Create(
                cmd.MetricKey, cmd.Scope, cmd.WarningLevel, cmd.CriticalLevel,
                cmd.ScopeId, cmd.CreatedBy);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.ThresholdId);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
