using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductCategories.Queries.GetProductCategories;

public record GetProductCategoriesQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<ProductCategoryDto>>;

public record ProductCategoryDto(
    int CategoryId,
    int? ParentId,
    string CategoryCode,
    string CategoryName,
    bool IsActive);
