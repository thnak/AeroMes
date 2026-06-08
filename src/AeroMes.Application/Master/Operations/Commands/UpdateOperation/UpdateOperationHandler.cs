using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.Operations.Commands.UpdateOperation;

public class UpdateOperationHandler(
    IOperationRepository repo,
    IUnitOfWork uow) : IRequestHandler<UpdateOperationCommand>
{
    public async Task Handle(UpdateOperationCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException("Operation", cmd.Code);
        entity.UpdateDetails(cmd.Name, cmd.Description);
        await uow.SaveChangesAsync(ct);
    }
}
