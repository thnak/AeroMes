using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.RemoveToolOperation;

public class RemoveToolOperationHandler(
    IToolRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveToolOperationCommand>
{
    public async Task HandleAsync(RemoveToolOperationCommand cmd, CancellationToken ct)
    {
        var tool = await repo.GetByCodeAsync(cmd.ToolCode, ct)
            ?? throw new EntityNotFoundException(nameof(Tool), cmd.ToolCode);

        tool.RemoveOperationMapping(cmd.MappingId, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
