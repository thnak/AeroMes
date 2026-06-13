using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.DetailedPlan.Commands.CreateDetailedPlan;

public class CreateDetailedPlanHandler(
    IDetailedProductionPlanRepository repo,
    IMasterProductionPlanRepository masterRepo,
    IUnitOfWork uow,
    IValidator<CreateDetailedPlanCommand> validator) : ICommandHandler<CreateDetailedPlanCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        CreateDetailedPlanCommand cmd, CancellationToken ct = default)
    {
        var v = await validator.ValidateAsync(cmd, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        var master = await masterRepo.GetByIdAsync(cmd.MasterPlanId, ct);
        if (master is null)
            return ValidationResult<int>.NotFound($"Master plan {cmd.MasterPlanId} not found.");

        var planNumber = string.IsNullOrWhiteSpace(cmd.PlanNumber)
            ? await repo.NextPlanNumberAsync(ct)
            : cmd.PlanNumber.Trim().ToUpperInvariant();

        try
        {
            var plan = DetailedProductionPlan.Create(
                planNumber, cmd.PlanName, cmd.MasterPlanId,
                master.OrganizationalUnit, master.PeriodStart, master.PeriodEnd,
                cmd.Granularity, cmd.CreatedBy);

            await repo.AddAsync(plan, ct);
            await uow.SaveChangesAsync(ct);

            foreach (var li in cmd.ProductLines)
            {
                var line = DppProductLine.Create(
                    plan.DetailPlanId, li.ProductCode, li.ProductName,
                    li.UnitOfMeasure, li.TotalRequiredQty, li.DailyCapacity);
                plan.AddProductLine(line);
            }

            if (cmd.ProductLines.Count > 0)
                await uow.SaveChangesAsync(ct);

            return ValidationResult<int>.Ok(plan.DetailPlanId);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
