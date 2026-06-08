using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.Operations.Commands.CreateOperation;

public class CreateOperationHandler(
    IOperationRepository repo,
    IUnitOfWork uow) : IRequestHandler<CreateOperationCommand, string>
{
    public async Task<string> Handle(CreateOperationCommand cmd, CancellationToken ct)
    {
        var entity = Operation.Create(cmd.Code, cmd.Name, cmd.Description);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.OperationCode;
    }
}
