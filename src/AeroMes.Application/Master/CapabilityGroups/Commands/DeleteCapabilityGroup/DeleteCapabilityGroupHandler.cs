using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.CapabilityGroups.Commands.DeleteCapabilityGroup;

public class DeleteCapabilityGroupHandler(
    ICapabilityGroupRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteCapabilityGroupCommand>
{
    public async Task HandleAsync(DeleteCapabilityGroupCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException("CapabilityGroup", cmd.Code);
        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
    }
}
