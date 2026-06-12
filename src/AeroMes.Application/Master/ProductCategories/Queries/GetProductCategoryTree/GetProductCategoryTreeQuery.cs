using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductCategories.Queries.GetProductCategoryTree;

public record GetProductCategoryTreeQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<ProductCategoryTreeDto>>;

public record ProductCategoryTreeDto(
    int CategoryId,
    string CategoryCode,
    string CategoryName,
    string? Description,
    decimal? StandardProductionTime,
    string? Color,
    bool IsActive,
    IReadOnlyList<ProductCategoryTreeDto> Children);
