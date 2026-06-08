using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.Routings.Commands.AddRoutingStep;

public class AddRoutingStepHandler(
    IRoutingRepository repo,
    IUnitOfWork uow) : IRequestHandler<AddRoutingStepCommand, int>
{
    public async Task<int> Handle(AddRoutingStepCommand cmd, CancellationToken ct)
    {
        if (!await repo.ExistsAsync(cmd.RoutingId, ct))
            throw new EntityNotFoundException("Routing", cmd.RoutingId);

        var step = RoutingStep.Create(
            cmd.RoutingId,
            cmd.StepNumber,
            cmd.OperationCode,
            cmd.DefaultWorkCenterId,
            cmd.StandardCycleTime,
            cmd.IsQcRequired);

        repo.AddStep(step);
        await uow.SaveChangesAsync(ct);
        return step.RoutingStepID;
    }
}
