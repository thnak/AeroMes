using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Master;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using DomainRules = AeroMes.Domain.Master.WorkOrderAutoRules;

namespace AeroMes.Application.Master.WorkOrderAutoRules.Commands.UpsertWorkOrderAutoRules;

public class UpsertWorkOrderAutoRulesHandler(
    IWorkOrderAutoRulesRepository repo,
    IUnitOfWork uow,
    IValidator<UpsertWorkOrderAutoRulesCommand> validator) : ICommandHandler<UpsertWorkOrderAutoRulesCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(UpsertWorkOrderAutoRulesCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            DomainRules entity;
            var existing = cmd.WorkCenterId.HasValue
                ? await repo.GetByWorkCenterAsync(cmd.WorkCenterId.Value, ct)
                : await repo.GetFactoryWideAsync(ct);
            if (existing is not null)
            {
                existing.UpdateDetails(cmd.AutoStartEnabled, cmd.AutoCompleteOnTargetReached,
                    cmd.RequireDeleteConfirmToken, cmd.MaxConcurrentJobs, cmd.RequireCertification, cmd.UpdatedBy);
                entity = existing;
            }
            else
            {
                entity = DomainRules.Create(
                    cmd.WorkCenterId, cmd.AutoStartEnabled, cmd.AutoCompleteOnTargetReached,
                    cmd.RequireDeleteConfirmToken, cmd.MaxConcurrentJobs, cmd.RequireCertification, cmd.UpdatedBy);
                await repo.AddAsync(entity, ct);
            }
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.RuleId);
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
