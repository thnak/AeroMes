using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.CapabilityGroups.Commands.CreateCapabilityGroup;

public class CreateCapabilityGroupHandler(
    ICapabilityGroupRepository repo,
    IUnitOfWork uow) : ICommandHandler<CreateCapabilityGroupCommand, string>
{
    public async Task<string> HandleAsync(CreateCapabilityGroupCommand cmd, CancellationToken ct)
    {
        var entity = CapabilityGroup.Create(cmd.Code, cmd.Name, cmd.Description);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.GroupCode;
    }
}
