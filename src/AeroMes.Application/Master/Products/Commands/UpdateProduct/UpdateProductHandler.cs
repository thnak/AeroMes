using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.Products.Commands.UpdateProduct;

public class UpdateProductHandler(
    IProductRepository repo,
    IUnitOfWork uow) : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException("Product", cmd.Code);
        entity.UpdateDetails(cmd.Name, cmd.Unit, cmd.IsFinishedGood, cmd.BarcodePattern, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
