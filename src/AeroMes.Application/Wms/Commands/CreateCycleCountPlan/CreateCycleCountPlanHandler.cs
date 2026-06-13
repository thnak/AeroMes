using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateCycleCountPlan;

public class CreateCycleCountPlanHandler(
    ICycleCountPlanRepository repo,
    IUnitOfWork uow,
    IValidator<CreateCycleCountPlanCommand> validator)
    : ICommandHandler<CreateCycleCountPlanCommand, ValidationResult<CycleCountPlanCreatedResult>>
{
    public async Task<ValidationResult<CycleCountPlanCreatedResult>> HandleAsync(
        CreateCycleCountPlanCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<CycleCountPlanCreatedResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            var suffix = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
            var planCode = $"CC-{DateTime.UtcNow:yyyyMMddHHmmss}-{suffix}";
            while (await repo.PlanCodeExistsAsync(planCode, ct))
            {
                suffix = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
                planCode = $"CC-{DateTime.UtcNow:yyyyMMddHHmmss}-{suffix}";
            }

            var plan = CycleCountPlan.Create(planCode, cmd.PlanType, cmd.ScheduledDate, cmd.Notes, cmd.CreatedBy);
            await repo.AddAsync(plan, ct);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<CycleCountPlanCreatedResult>.Ok(new(plan.PlanId, plan.PlanCode));
        }
        catch (DomainException ex)
        {
            return ValidationResult<CycleCountPlanCreatedResult>.Failure(ex.Message);
        }
    }
}
