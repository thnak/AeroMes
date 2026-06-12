using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.ScrapTool;

public class ScrapToolHandler(
    IToolRepository repo,
    IUnitOfWork uow) : ICommandHandler<ScrapToolCommand>
{
    public async Task HandleAsync(ScrapToolCommand cmd, CancellationToken ct)
    {
        var tool = await repo.GetByCodeAsync(cmd.ToolCode, ct)
            ?? throw new EntityNotFoundException(nameof(Tool), cmd.ToolCode);

        tool.Scrap(cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
