using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.CapabilityGroups.Commands.UpdateCapabilityGroup;

public class UpdateCapabilityGroupHandler(
    ICapabilityGroupRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateCapabilityGroupCommand>
{
    public async Task HandleAsync(UpdateCapabilityGroupCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException("CapabilityGroup", cmd.Code);
        entity.UpdateDetails(cmd.Name, cmd.Description, cmd.IsActive, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
