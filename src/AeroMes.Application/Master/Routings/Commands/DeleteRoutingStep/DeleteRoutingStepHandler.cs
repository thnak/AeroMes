using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Routings.Commands.DeleteRoutingStep;

public class DeleteRoutingStepHandler(
    IRoutingRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteRoutingStepCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteRoutingStepCommand cmd, CancellationToken ct)
    {
        var step = await repo.GetStepByIdAsync(cmd.RoutingStepId, ct);
        if (step is null) return ValidationResult<Unit>.NotFound($"RoutingStep '{cmd.RoutingStepId}' was not found.");
        repo.RemoveSteps([step]);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
