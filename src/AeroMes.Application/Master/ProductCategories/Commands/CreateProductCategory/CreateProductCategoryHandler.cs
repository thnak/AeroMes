using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductCategories.Commands.CreateProductCategory;

public class CreateProductCategoryHandler(
    IProductCategoryRepository repo,
    IUnitOfWork uow) : ICommandHandler<CreateProductCategoryCommand, int>
{
    public async Task<int> HandleAsync(CreateProductCategoryCommand cmd, CancellationToken ct)
    {
        var entity = ProductCategory.Create(
            cmd.ParentId, cmd.Code, cmd.Name,
            cmd.Description, cmd.StandardProductionTime, cmd.Color,
            cmd.CreatedBy);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.CategoryId;
    }
}
