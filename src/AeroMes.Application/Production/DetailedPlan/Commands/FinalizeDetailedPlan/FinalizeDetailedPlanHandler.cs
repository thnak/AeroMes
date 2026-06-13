using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.DetailedPlan.Commands.FinalizeDetailedPlan;

public class FinalizeDetailedPlanHandler(
    IDetailedProductionPlanRepository repo,
    IUnitOfWork uow) : ICommandHandler<FinalizeDetailedPlanCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        FinalizeDetailedPlanCommand cmd, CancellationToken ct = default)
    {
        var plan = await repo.GetByIdAsync(cmd.DetailPlanId, ct);
        if (plan is null) return ValidationResult<Unit>.NotFound($"Detailed plan {cmd.DetailPlanId} not found.");

        try { plan.Finalize(cmd.UpdatedBy); }
        catch (DomainException ex) { return ValidationResult<Unit>.Failure(ex.Message); }

        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
