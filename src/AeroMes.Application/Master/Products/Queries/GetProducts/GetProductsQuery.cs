using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Products.Queries.GetProducts;

public record GetProductsQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<ProductDto>>;

public record ProductDto(
    string ProductCode,
    string ProductName,
    string ProductUnit,
    bool IsFinishedGood,
    string? BarcodePattern,
    bool IsActive);
