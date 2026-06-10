using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Products.Queries.GetProducts;

public class GetProductsHandler(IProductRepository repo)
    : IQueryHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> HandleAsync(GetProductsQuery q, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(q.ActiveOnly, ct);
        return items
            .Where(x => q.ItemType == null || x.ItemType == q.ItemType)
            .Where(x => q.CategoryId == null || x.CategoryId == q.CategoryId)
            .Where(x => q.LifecycleStatus == null || x.LifecycleStatus == q.LifecycleStatus)
            .Select(x => new ProductDto(
                x.ProductCode, x.ProductName, x.BaseUoMCode, x.ItemType,
                x.CategoryId, x.LifecycleStatus, x.LotControlled, x.SerialControlled,
                x.ProcurementType, x.IsActive, x.BarcodePattern,
                x.CustomerPartNo, x.DrawingNo, x.Revision))
            .ToList();
    }
}
