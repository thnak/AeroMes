using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Queries.GetProductAttributes;

public class GetProductAttributesHandler(IProductAttributeRepository repo)
    : IQueryHandler<GetProductAttributesQuery, IReadOnlyList<ProductAttributeDto>>
{
    public async Task<IReadOnlyList<ProductAttributeDto>> HandleAsync(GetProductAttributesQuery q, CancellationToken ct)
    {
        var attributes = await repo.GetAllAsync(q.ActiveOnly, q.Search, ct);
        return [.. attributes.Select(a => new ProductAttributeDto(
            a.AttributeId, a.AttributeCode, a.AttributeName, a.IsActive, a.Values.Count))];
    }
}
