using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.UnitOfMeasures.Commands.UpdateUoM;

public class UpdateUoMHandler(
    IUnitOfMeasureRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateUoMCommand>
{
    public async Task HandleAsync(UpdateUoMCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException("UnitOfMeasure", cmd.Code);
        entity.Update(cmd.Name, cmd.Group);
        await uow.SaveChangesAsync(ct);
    }
}
