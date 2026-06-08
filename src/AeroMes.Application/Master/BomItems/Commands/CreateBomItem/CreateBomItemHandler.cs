using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.BomItems.Commands.CreateBomItem;

public class CreateBomItemHandler(
    IBomItemRepository repo,
    IUnitOfWork uow) : IRequestHandler<CreateBomItemCommand, int>
{
    public async Task<int> Handle(CreateBomItemCommand cmd, CancellationToken ct)
    {
        var entity = BomItem.Create(
            cmd.ParentProductCode,
            cmd.ChildProductCode,
            cmd.RequiredQty,
            cmd.ScrapFactor,
            cmd.CreatedBy);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.BomID;
    }
}
