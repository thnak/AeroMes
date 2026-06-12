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

        if (await repo.IsReferencedAsync(cmd.Code, ct))
            throw new DomainException(
                $"Sản phẩm '{entity.ProductCode}' đang được tham chiếu bởi BOM, routing hoặc lệnh sản xuất — chỉ có thể vô hiệu hóa, không thể xóa.");

        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
    }
}
