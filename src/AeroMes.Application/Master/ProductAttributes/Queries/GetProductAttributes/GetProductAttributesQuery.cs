using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Queries.GetProductAttributes;

public record GetProductAttributesQuery(bool ActiveOnly = true, string? Search = null)
    : IQuery<IReadOnlyList<ProductAttributeDto>>;

public record ProductAttributeDto(
    int AttributeId,
    string AttributeCode,
    string AttributeName,
    bool IsActive,
    int ValueCount);
