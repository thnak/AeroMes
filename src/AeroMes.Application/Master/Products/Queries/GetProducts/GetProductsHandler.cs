using AeroMes.Domain.Master.Repositories;
using MediatR;

namespace AeroMes.Application.Master.Products.Queries.GetProducts;

public class GetProductsHandler(IProductRepository repo)
    : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> Handle(GetProductsQuery q, CancellationToken ct)
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
