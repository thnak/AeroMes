using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.BomItems.Commands.DeleteBomItem;

public class DeleteBomItemHandler(
    IBomItemRepository repo,
    IUnitOfWork uow) : IRequestHandler<DeleteBomItemCommand>
{
    public async Task Handle(DeleteBomItemCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.BomId, ct)
            ?? throw new EntityNotFoundException("BomItem", cmd.BomId);
        entity.Deactivate();
        await uow.SaveChangesAsync(ct);
    }
}
