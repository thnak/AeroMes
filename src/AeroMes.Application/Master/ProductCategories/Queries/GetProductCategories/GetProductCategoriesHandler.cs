using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductCategories.Queries.GetProductCategories;

public class GetProductCategoriesHandler(IProductCategoryRepository repo)
    : IQueryHandler<GetProductCategoriesQuery, IReadOnlyList<ProductCategoryDto>>
{
    public async Task<IReadOnlyList<ProductCategoryDto>> HandleAsync(GetProductCategoriesQuery q, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(q.ActiveOnly, ct);
        return items.Select(x => new ProductCategoryDto(
            x.CategoryId, x.ParentId, x.CategoryCode, x.CategoryName,
            x.Description, x.StandardProductionTime, x.Color, x.IsActive)).ToList();
    }
}
