using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.SendToolForService;

public class SendToolForServiceHandler(
    IToolRepository repo,
    IUnitOfWork uow) : ICommandHandler<SendToolForServiceCommand>
{
    public async Task HandleAsync(SendToolForServiceCommand cmd, CancellationToken ct)
    {
        var tool = await repo.GetByCodeAsync(cmd.ToolCode, ct)
            ?? throw new EntityNotFoundException(nameof(Tool), cmd.ToolCode);

        tool.SendForService(cmd.ServiceType, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
