using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.Products.Commands.DeleteProduct;

public class DeleteProductHandler(
    IProductRepository repo,
    IUnitOfWork uow) : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException("Product", cmd.Code);
        entity.Deactivate();
        await uow.SaveChangesAsync(ct);
    }
}
