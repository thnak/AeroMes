using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Products.Queries.GetProducts;

public class GetProductsHandler(IProductRepository repo)
    : IQueryHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> HandleAsync(GetProductsQuery q, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(q.ActiveOnly, ct);
        return items.Select(x => new ProductDto(
            x.ProductCode,
            x.ProductName,
            x.ProductUnit,
            x.IsFinishedGood,
            x.BarcodePattern,
            x.IsActive)).ToList();
    }
}
