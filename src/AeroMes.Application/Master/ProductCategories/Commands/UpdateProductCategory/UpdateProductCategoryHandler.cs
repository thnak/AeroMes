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
        entity.UpdateDetails(cmd.ParentId, cmd.Name, cmd.IsActive, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
