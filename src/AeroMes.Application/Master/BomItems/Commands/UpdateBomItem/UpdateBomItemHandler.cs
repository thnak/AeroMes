using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.BomItems.Commands.UpdateBomItem;

public class UpdateBomItemHandler(
    IBomItemRepository repo,
    IUnitOfWork uow) : IRequestHandler<UpdateBomItemCommand>
{
    public async Task Handle(UpdateBomItemCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.BomId, ct)
            ?? throw new EntityNotFoundException("BomItem", cmd.BomId);
        entity.UpdateQty(cmd.RequiredQty, cmd.ScrapFactor, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
