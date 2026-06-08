using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.Machines.Commands.CreateMachine;

public class CreateMachineHandler(
    IMachineRepository repo,
    IUnitOfWork uow) : IRequestHandler<CreateMachineCommand, string>
{
    public async Task<string> Handle(CreateMachineCommand cmd, CancellationToken ct)
    {
        var entity = Machine.Create(cmd.Code, cmd.Name, cmd.WorkCenterId, cmd.Brand, cmd.Model, cmd.CreatedBy);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.MachineCode;
    }
}
