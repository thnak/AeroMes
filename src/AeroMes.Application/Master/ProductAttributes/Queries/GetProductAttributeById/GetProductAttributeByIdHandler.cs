using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Queries.GetProductAttributeById;

public class GetProductAttributeByIdHandler(IProductAttributeRepository repo)
    : IQueryHandler<GetProductAttributeByIdQuery, ProductAttributeDetailDto?>
{
    public async Task<ProductAttributeDetailDto?> HandleAsync(GetProductAttributeByIdQuery q, CancellationToken ct)
    {
        var attribute = await repo.GetByIdWithValuesAsync(q.AttributeId, ct);
        if (attribute is null) return null;

        return new ProductAttributeDetailDto(
            attribute.AttributeId,
            attribute.AttributeCode,
            attribute.AttributeName,
            attribute.IsActive,
            [.. attribute.Values
                .OrderBy(v => v.SortOrder).ThenBy(v => v.Value)
                .Select(v => new ProductAttributeValueDto(v.ValueId, v.Value, v.GroupName, v.SortOrder))]);
    }
}
