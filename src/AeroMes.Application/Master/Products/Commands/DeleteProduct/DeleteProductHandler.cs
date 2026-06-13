using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Products.Commands.DeleteProduct;

public class DeleteProductHandler(
    IProductRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteProductCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteProductCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"Product '{cmd.Code}' was not found.");

        if (await repo.IsReferencedAsync(cmd.Code, ct))
            throw new DomainException(
                $"Sản phẩm '{entity.ProductCode}' đang được tham chiếu bởi BOM, routing hoặc lệnh sản xuất — chỉ có thể vô hiệu hóa, không thể xóa.");

        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
