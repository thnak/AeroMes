using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Queries.GetProductAttributeById;

public record GetProductAttributeByIdQuery(int AttributeId) : IQuery<ProductAttributeDetailDto?>;

public record ProductAttributeDetailDto(
    int AttributeId,
    string AttributeCode,
    string AttributeName,
    bool IsActive,
    IReadOnlyList<ProductAttributeValueDto> Values);

public record ProductAttributeValueDto(
    int ValueId,
    string Value,
    string? GroupName,
    int SortOrder);
