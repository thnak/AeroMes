using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.UnitOfMeasures.Commands.CreateUoM;

public class CreateUoMHandler(
    IUnitOfMeasureRepository repo,
    IUnitOfWork uow) : ICommandHandler<CreateUoMCommand, string>
{
    public async Task<string> HandleAsync(CreateUoMCommand cmd, CancellationToken ct)
    {
        var entity = UnitOfMeasure.Create(cmd.Code, cmd.Name, cmd.Group);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.UoMCode;
    }
}
