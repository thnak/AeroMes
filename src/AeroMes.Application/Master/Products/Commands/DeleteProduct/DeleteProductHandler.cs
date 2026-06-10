using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.DeleteProduct;

public class DeleteProductHandler(
    IProductRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteProductCommand>
{
    public async Task HandleAsync(DeleteProductCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException("Product", cmd.Code);
        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
    }
}
