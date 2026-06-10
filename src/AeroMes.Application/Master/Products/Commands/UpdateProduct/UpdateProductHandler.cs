using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.UpdateProduct;

public class UpdateProductHandler(
    IProductRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateProductCommand>
{
    public async Task HandleAsync(UpdateProductCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException("Product", cmd.Code);
        entity.UpdateDetails(
            cmd.Name, cmd.BaseUoMCode, cmd.PurchaseUoMCode, cmd.PurchaseToBaseQty,
            cmd.ItemType, cmd.CategoryId, cmd.BarcodePattern,
            cmd.LotControlled, cmd.SerialControlled, cmd.ShelfLifeDays,
            cmd.ReorderPoint, cmd.SafetyStock, cmd.LeadTimeDays, cmd.ProcurementType,
            cmd.EffectiveFrom, cmd.EffectiveTo,
            cmd.CustomerPartNo, cmd.DrawingNo, cmd.Revision,
            cmd.NetWeight, cmd.GrossWeight, cmd.Length, cmd.Width, cmd.Height,
            cmd.ImageUrl, cmd.ThumbnailUrl, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
