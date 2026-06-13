using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductFamilies.Queries.GetProductFamilies;

public sealed class GetProductFamiliesHandler(IProductFamilyRepository repo)
    : IQueryHandler<GetProductFamiliesQuery, IReadOnlyList<ProductFamilySummaryDto>>
{
    public async Task<IReadOnlyList<ProductFamilySummaryDto>> HandleAsync(GetProductFamiliesQuery query, CancellationToken ct)
    {
        var families = await repo.GetAllAsync(query.Industry, query.IsActive, ct);
        return families.Select(f => new ProductFamilySummaryDto(
            f.FamilyCode, f.FamilyName, f.BaseProductCode, f.Industry, f.IsActive,
            f.Dimensions.Count, f.Variants.Count)).ToList();
    }
}
