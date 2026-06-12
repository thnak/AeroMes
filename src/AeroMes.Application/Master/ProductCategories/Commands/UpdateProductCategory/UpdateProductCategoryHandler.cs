using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductCategories.Commands.UpdateProductCategory;

public class UpdateProductCategoryHandler(
    IProductCategoryRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateProductCategoryCommand>
{
    public async Task HandleAsync(UpdateProductCategoryCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new EntityNotFoundException("ProductCategory", cmd.Id);

        if (cmd.ParentId is int parentId)
            await EnsureNoCycleAsync(cmd.Id, parentId, ct);

        entity.UpdateDetails(
            cmd.ParentId, cmd.Name,
            cmd.Description, cmd.StandardProductionTime, cmd.Color,
            cmd.IsActive, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }

    private async Task EnsureNoCycleAsync(int categoryId, int parentId, CancellationToken ct)
    {
        var current = (int?)parentId;
        while (current is int id)
        {
            if (id == categoryId)
                throw new DomainException("Nhóm cha không hợp lệ — tạo vòng lặp trong cây phân cấp.");
            var parent = await repo.GetByIdAsync(id, ct)
                ?? throw new EntityNotFoundException("ProductCategory", id);
            current = parent.ParentId;
        }
    }
}
