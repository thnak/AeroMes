using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.Operations.Commands.DeleteOperation;

public class DeleteOperationHandler(
    IOperationRepository repo,
    IUnitOfWork uow) : IRequestHandler<DeleteOperationCommand>
{
    public async Task Handle(DeleteOperationCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException("Operation", cmd.Code);
        entity.Deactivate();
        await uow.SaveChangesAsync(ct);
    }
}
