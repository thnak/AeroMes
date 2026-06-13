using AeroMes.Application.Common;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CreateProductionPlan;

public class CreateProductionPlanHandler(
    IProductionPlanByOrderRepository planRepo,
    IProductionTeamRepository teamRepo,
    IProductRepository productRepo,
    IValidator<CreateProductionPlanCommand> validator)
    : ICommandHandler<CreateProductionPlanCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        CreateProductionPlanCommand cmd, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(cmd, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        var planCode = $"PLAN-{DateTime.UtcNow:yyyyMMddHHmmss}";
        if (await planRepo.CodeExistsAsync(planCode, ct))
            planCode += "-1";

        var plan = ProductionPlanByOrder.Create(planCode, cmd.PoId, cmd.AllocationMethod, cmd.Notes, null);

        if (cmd.AllocationMethod == PlanAllocationMethod.Auto)
        {
            var teams = await teamRepo.GetAllAsync(activeOnly: true, search: null, orgUnitId: null, ct);
            var enabledTeams = teams.Where(t => t.IsOrderBasedPlanningEnabled).ToList();

            foreach (var lineInput in cmd.Lines)
            {
                var product = await productRepo.GetByCodeAsync(lineInput.ProductCode, ct);
                string? assignedTeam = null;
                DateTime? start = null, end = null;

                if (product is not null)
                {
                    var compatible = enabledTeams
                        .Where(t => product.CategoryId.HasValue
                            && t.ProductGroups.Any(pg => pg.CategoryId == product.CategoryId.Value))
                        .FirstOrDefault();

                    if (compatible is not null)
                    {
                        assignedTeam = compatible.TeamCode;
                        start = DateTime.UtcNow.Date.AddDays(1);

                        if (compatible.StandardLaborQuantity.HasValue && compatible.ProductionRate.HasValue
                            && compatible.StandardLaborQuantity.Value > 0 && compatible.ProductionRate.Value > 0)
                        {
                            var hoursNeeded = (double)(lineInput.PlannedQty
                                / (compatible.StandardLaborQuantity.Value * compatible.ProductionRate.Value));
                            end = start.Value.AddHours(hoursNeeded);
                        }
                    }
                }

                plan.AddLine(lineInput.ProductCode, lineInput.PlannedQty, assignedTeam, start, end);
            }
        }
        else
        {
            foreach (var lineInput in cmd.Lines)
                plan.AddLine(lineInput.ProductCode, lineInput.PlannedQty,
                    lineInput.TeamCode, lineInput.PlannedStartDate, lineInput.PlannedEndDate);
        }

        await planRepo.AddAsync(plan, ct);
        await planRepo.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(plan.PlanId);
    }
}
