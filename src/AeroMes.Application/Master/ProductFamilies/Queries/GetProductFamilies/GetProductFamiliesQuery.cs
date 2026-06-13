using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductFamilies.Queries.GetProductFamilies;

public sealed record GetProductFamiliesQuery(string? Industry, bool? IsActive)
    : IQuery<IReadOnlyList<ProductFamilySummaryDto>>;

public sealed record ProductFamilySummaryDto(
    string FamilyCode,
    string FamilyName,
    string BaseProductCode,
    string Industry,
    bool IsActive,
    int DimensionCount,
    int VariantCount);
