using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.MachineProductConfigs.Commands.DeleteMachineProductConfig;

public class DeleteMachineProductConfigHandler(
    IMachineProductConfigRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteMachineProductConfigCommand>
{
    public async Task HandleAsync(DeleteMachineProductConfigCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetAsync(cmd.MachineCode, cmd.ProductCode, ct)
            ?? throw new EntityNotFoundException("MachineProductConfig", $"{cmd.MachineCode}/{cmd.ProductCode}");

        repo.Remove(entity);
        await uow.SaveChangesAsync(ct);
    }
}
