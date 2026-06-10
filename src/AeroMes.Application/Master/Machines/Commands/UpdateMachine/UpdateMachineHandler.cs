using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Machines.Commands.UpdateMachine;

public class UpdateMachineHandler(
    IMachineRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateMachineCommand>
{
    public async Task HandleAsync(UpdateMachineCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException("Machine", cmd.Code);
        entity.UpdateDetails(cmd.Name, cmd.WorkCenterId, cmd.Brand, cmd.Model, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
