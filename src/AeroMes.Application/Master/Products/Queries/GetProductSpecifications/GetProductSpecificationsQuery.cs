using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Products.Queries.GetProductSpecifications;

public record GetProductSpecificationsQuery(string ProductCode) : IQuery<IReadOnlyList<ProductSpecificationDto>?>;

public record ProductSpecificationDto(
    int SpecificationId,
    string SpecCode,
    string? Description,
    bool IsActive);
