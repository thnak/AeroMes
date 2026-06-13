using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.ProductCategories.Commands.DeleteProductCategory;

public class DeleteProductCategoryHandler(
    IProductCategoryRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteProductCategoryCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteProductCategoryCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.Id, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"ProductCategory '{cmd.Id}' was not found.");

        if (await repo.HasProductsAsync(cmd.Id, ct))
            throw new DomainException(
                $"Nhóm '{entity.CategoryCode}' đang được sản phẩm sử dụng — chỉ có thể vô hiệu hóa, không thể xóa.");

        if (await repo.HasChildrenAsync(cmd.Id, ct))
            throw new DomainException(
                $"Nhóm '{entity.CategoryCode}' còn nhóm con — xóa hoặc di chuyển nhóm con trước.");

        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
