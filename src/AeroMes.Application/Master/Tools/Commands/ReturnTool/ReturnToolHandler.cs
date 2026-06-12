using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.ReturnTool;

public class ReturnToolHandler(
    IToolRepository repo,
    IUnitOfWork uow) : ICommandHandler<ReturnToolCommand>
{
    public async Task HandleAsync(ReturnToolCommand cmd, CancellationToken ct)
    {
        var tool = await repo.GetByCodeAsync(cmd.ToolCode, ct)
            ?? throw new EntityNotFoundException(nameof(Tool), cmd.ToolCode);

        tool.Return(cmd.ReturnedBy, cmd.Condition, cmd.Notes, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
