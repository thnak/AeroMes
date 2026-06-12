using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Products.Queries.GetProductVariants;

public class GetProductVariantsHandler(IProductRepository repo)
    : IQueryHandler<GetProductVariantsQuery, IReadOnlyList<ProductVariantDto>>
{
    public async Task<IReadOnlyList<ProductVariantDto>> HandleAsync(GetProductVariantsQuery q, CancellationToken ct)
    {
        var variants = await repo.GetVariantsAsync(q.ParentProductCode, ct);
        return [.. variants.Select(v => new ProductVariantDto(
            v.ProductCode, v.ProductName, v.LifecycleStatus, v.IsActive))];
    }
}
