using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductCategories.Queries.GetProductCategoryTree;

public class GetProductCategoryTreeHandler(IProductCategoryRepository repo)
    : IQueryHandler<GetProductCategoryTreeQuery, IReadOnlyList<ProductCategoryTreeDto>>
{
    public async Task<IReadOnlyList<ProductCategoryTreeDto>> HandleAsync(GetProductCategoryTreeQuery q, CancellationToken ct)
    {
        var all = await repo.GetAllAsync(q.ActiveOnly, ct);
        var byParent = all.ToLookup(x => x.ParentId);
        // Categories whose parent is filtered out (inactive) surface as roots so nothing disappears from the tree.
        var ids = all.Select(x => x.CategoryId).ToHashSet();
        var roots = all.Where(x => x.ParentId is null || !ids.Contains(x.ParentId.Value));
        return [.. roots.Select(x => ToNode(x, byParent))];
    }

    private static ProductCategoryTreeDto ToNode(ProductCategory category, ILookup<int?, ProductCategory> byParent) =>
        new(category.CategoryId,
            category.CategoryCode,
            category.CategoryName,
            category.Description,
            category.StandardProductionTime,
            category.Color,
            category.IsActive,
            [.. byParent[category.CategoryId].Select(c => ToNode(c, byParent))]);
}
