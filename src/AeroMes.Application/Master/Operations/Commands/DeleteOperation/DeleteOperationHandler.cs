using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Operations.Commands.DeleteOperation;

public class DeleteOperationHandler(
    IOperationRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteOperationCommand>
{
    public async Task HandleAsync(DeleteOperationCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException("Operation", cmd.Code);
        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
    }
}
