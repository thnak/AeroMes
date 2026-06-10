using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.CreateProduct;

public class CreateProductHandler(
    IProductRepository repo,
    IUnitOfWork uow) : ICommandHandler<CreateProductCommand, string>
{
    public async Task<string> HandleAsync(CreateProductCommand cmd, CancellationToken ct)
    {
        var entity = Product.Create(
            cmd.Code, cmd.Name, cmd.BaseUoMCode, cmd.ItemType, cmd.CategoryId,
            cmd.BarcodePattern, cmd.LotControlled, cmd.SerialControlled, cmd.ShelfLifeDays,
            cmd.ProcurementType, cmd.CustomerPartNo, cmd.DrawingNo, cmd.Revision, cmd.CreatedBy);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.ProductCode;
    }
}
