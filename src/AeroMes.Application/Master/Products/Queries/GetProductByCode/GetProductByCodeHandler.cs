using AeroMes.Application.Master.Products.Queries.GetProducts;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Products.Queries.GetProductByCode;

public class GetProductByCodeHandler(IProductRepository repo)
    : IQueryHandler<GetProductByCodeQuery, ProductDetailDto?>
{
    public async Task<ProductDetailDto?> HandleAsync(GetProductByCodeQuery q, CancellationToken ct)
    {
        var x = await repo.GetByCodeWithConversionsAsync(q.Code, ct);
        if (x is null) return null;
        return new ProductDetailDto(
            x.ProductCode, x.ProductName, x.BaseUoMCode, x.PurchaseUoMCode, x.PurchaseToBaseQty,
            x.ItemType, x.CategoryId, x.LifecycleStatus,
            x.LotControlled, x.SerialControlled, x.ShelfLifeDays,
            x.ReorderPoint, x.SafetyStock, x.LeadTimeDays, x.ProcurementType,
            x.EffectiveFrom, x.EffectiveTo,
            x.CustomerPartNo, x.DrawingNo, x.Revision,
            x.NetWeight, x.GrossWeight, x.Length, x.Width, x.Height,
            x.ImageUrl, x.ThumbnailUrl, x.IsActive, x.BarcodePattern,
            x.FixedPurchasePrice, x.TechnicalStandard, x.QuantityFormula,
            [.. x.UoMConversions
                .OrderBy(c => c.UoMCode)
                .Select(c => new ProductUoMConversionDto(c.ConversionId, c.UoMCode, c.ToBaseFactor, c.Notes))]);
    }
}
