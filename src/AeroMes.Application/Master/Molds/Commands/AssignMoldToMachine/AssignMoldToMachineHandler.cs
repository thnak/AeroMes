using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.AssignMoldToMachine;

public class AssignMoldToMachineHandler(
    IMoldRepository repo,
    IUnitOfWork uow) : ICommandHandler<AssignMoldToMachineCommand>
{
    public async Task HandleAsync(AssignMoldToMachineCommand cmd, CancellationToken ct)
    {
        var mold = await repo.GetByCodeAsync(cmd.MoldCode, ct)
            ?? throw new EntityNotFoundException(nameof(Mold), cmd.MoldCode);

        mold.AssignToMachine(cmd.MachineCode, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
