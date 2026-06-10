using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Machines.Commands.DeleteMachine;

public class DeleteMachineHandler(
    IMachineRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteMachineCommand>
{
    public async Task HandleAsync(DeleteMachineCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException("Machine", cmd.Code);
        entity.Deactivate();
        await uow.SaveChangesAsync(ct);
    }
}
