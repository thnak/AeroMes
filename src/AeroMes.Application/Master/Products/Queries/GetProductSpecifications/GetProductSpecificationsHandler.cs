using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Products.Queries.GetProductSpecifications;

public class GetProductSpecificationsHandler(IProductRepository repo)
    : IQueryHandler<GetProductSpecificationsQuery, IReadOnlyList<ProductSpecificationDto>?>
{
    public async Task<IReadOnlyList<ProductSpecificationDto>?> HandleAsync(GetProductSpecificationsQuery q, CancellationToken ct)
    {
        var product = await repo.GetByCodeWithSpecificationsAsync(q.ProductCode, ct);
        if (product is null) return null;
        return [.. product.Specifications
            .OrderBy(s => s.SpecCode)
            .Select(s => new ProductSpecificationDto(s.SpecificationId, s.SpecCode, s.Description, s.IsActive))];
    }
}
