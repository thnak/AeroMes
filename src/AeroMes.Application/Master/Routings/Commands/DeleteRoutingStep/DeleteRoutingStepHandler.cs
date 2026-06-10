using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Routings.Commands.DeleteRoutingStep;

public class DeleteRoutingStepHandler(
    IRoutingRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteRoutingStepCommand>
{
    public async Task HandleAsync(DeleteRoutingStepCommand cmd, CancellationToken ct)
    {
        var step = await repo.GetStepByIdAsync(cmd.RoutingStepId, ct)
            ?? throw new EntityNotFoundException("RoutingStep", cmd.RoutingStepId);
        repo.RemoveSteps([step]);
        await uow.SaveChangesAsync(ct);
    }
}
