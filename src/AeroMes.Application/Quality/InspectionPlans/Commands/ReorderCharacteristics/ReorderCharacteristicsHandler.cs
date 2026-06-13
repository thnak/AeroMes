using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionPlans.Commands.ReorderCharacteristics;

public class ReorderCharacteristicsHandler(
    IInspectionPlanRepository planRepo,
    IInspectionCharacteristicRepository charRepo,
    IUnitOfWork uow) : ICommandHandler<ReorderCharacteristicsCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(ReorderCharacteristicsCommand cmd, CancellationToken ct)
    {
        if (cmd.CharIds is null || cmd.CharIds.Count == 0)
            return ValidationResult<Unit>.Failure("CharIds list cannot be empty.");

        var plan = await planRepo.GetByIdAsync(cmd.PlanId, ct);
        if (plan is null)
            return ValidationResult<Unit>.NotFound($"Inspection plan {cmd.PlanId} not found.");

        var characteristics = await charRepo.GetByPlanIdAsync(cmd.PlanId, ct);
        var charMap = characteristics.ToDictionary(c => c.CharId);

        // Verify all provided IDs belong to this plan
        foreach (var id in cmd.CharIds)
        {
            if (!charMap.ContainsKey(id))
                return ValidationResult<Unit>.Failure($"Characteristic {id} does not belong to plan {cmd.PlanId}.");
        }

        // Assign sequence 1-based according to the order of charIds
        for (var i = 0; i < cmd.CharIds.Count; i++)
        {
            charMap[cmd.CharIds[i]].Resequence(i + 1);
        }

        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
