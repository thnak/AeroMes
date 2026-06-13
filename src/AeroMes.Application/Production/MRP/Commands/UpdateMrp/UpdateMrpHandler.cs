using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.MRP.Commands.UpdateMrp;

public class UpdateMrpHandler(IMaterialRequirementsPlanRepository repository)
    : ICommandHandler<UpdateMrpCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateMrpCommand command, CancellationToken ct)
    {
        var plan = await repository.GetByIdAsync(command.MrpID, ct);
        if (plan is null) return ValidationResult<Unit>.NotFound("Kế hoạch NVL không tồn tại.");

        try
        {
            plan.Update(command.PlanName, command.OrganizationalUnit,
                command.PeriodStart, command.PeriodEnd, command.Notes, command.UpdatedBy);

            if (command.Lines is not null)
            {
                var lines = command.Lines.Select(l =>
                {
                    var line = MrpLine.Create(
                        plan.MrpID, l.FinishedGoodCode, l.FinishedGoodQty,
                        l.MaterialCode, l.MaterialName, l.UnitOfMeasure,
                        l.FixedWaste, l.WasteRatio,
                        l.OpeningInventory, l.ConcurrentPurchaseRequestQty);
                    line.AdjustPlannedOrderQty(l.PlannedOrderQty);
                    return line;
                });
                plan.SetLines(lines, command.UpdatedBy);
            }

            await repository.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
