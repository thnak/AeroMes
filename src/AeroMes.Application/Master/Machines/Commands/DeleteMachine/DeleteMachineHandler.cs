using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.Machines.Commands.DeleteMachine;

public class DeleteMachineHandler(
    IMachineRepository repo,
    IUnitOfWork uow) : IRequestHandler<DeleteMachineCommand>
{
    public async Task Handle(DeleteMachineCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException("Machine", cmd.Code);
        entity.Deactivate();
        await uow.SaveChangesAsync(ct);
    }
}
