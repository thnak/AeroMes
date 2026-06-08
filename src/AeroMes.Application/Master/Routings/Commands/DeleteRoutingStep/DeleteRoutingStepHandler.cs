using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.Routings.Commands.DeleteRoutingStep;

public class DeleteRoutingStepHandler(
    IRoutingRepository repo,
    IUnitOfWork uow) : IRequestHandler<DeleteRoutingStepCommand>
{
    public async Task Handle(DeleteRoutingStepCommand cmd, CancellationToken ct)
    {
        var step = await repo.GetStepByIdAsync(cmd.RoutingStepId, ct)
            ?? throw new EntityNotFoundException("RoutingStep", cmd.RoutingStepId);
        repo.RemoveSteps([step]);
        await uow.SaveChangesAsync(ct);
    }
}
