using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.MasterPlan.Commands.ApproveMasterPlan;

public class ApproveMasterPlanHandler(
    IMasterProductionPlanRepository repo,
    IUnitOfWork uow) : ICommandHandler<ApproveMasterPlanCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        ApproveMasterPlanCommand cmd, CancellationToken ct = default)
    {
        var plan = await repo.GetByIdAsync(cmd.MasterPlanId, ct);
        if (plan is null) return ValidationResult<Unit>.NotFound($"Master plan {cmd.MasterPlanId} not found.");

        try { plan.Approve(cmd.ApprovedBy); }
        catch (DomainException ex) { return ValidationResult<Unit>.Failure(ex.Message); }

        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
