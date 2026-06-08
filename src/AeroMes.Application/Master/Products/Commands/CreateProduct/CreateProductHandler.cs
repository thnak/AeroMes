using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.Products.Commands.CreateProduct;

public class CreateProductHandler(
    IProductRepository repo,
    IUnitOfWork uow) : IRequestHandler<CreateProductCommand, string>
{
    public async Task<string> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        var entity = Product.Create(cmd.Code, cmd.Name, cmd.Unit, cmd.IsFinishedGood, cmd.BarcodePattern, cmd.CreatedBy);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.ProductCode;
    }
}
