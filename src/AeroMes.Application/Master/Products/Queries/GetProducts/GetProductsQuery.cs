using AeroMes.Domain.Master;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Products.Queries.GetProducts;

public record GetProductsQuery(
    bool ActiveOnly = true,
    ItemType? ItemType = null,
    int? CategoryId = null,
    LifecycleStatus? LifecycleStatus = null) : IQuery<IReadOnlyList<ProductDto>>;

public record ProductDto(
    string ProductCode,
    string ProductName,
    string BaseUoMCode,
    ItemType ItemType,
    int? CategoryId,
    LifecycleStatus LifecycleStatus,
    bool LotControlled,
    bool SerialControlled,
    ProcurementType? ProcurementType,
    bool IsActive,
    string? BarcodePattern,
    string? CustomerPartNo,
    string? DrawingNo,
    string? Revision);

public record ProductDetailDto(
    string ProductCode,
    string ProductName,
    string BaseUoMCode,
    string? PurchaseUoMCode,
    decimal PurchaseToBaseQty,
    ItemType ItemType,
    int? CategoryId,
    LifecycleStatus LifecycleStatus,
    bool LotControlled,
    bool SerialControlled,
    int? ShelfLifeDays,
    decimal? ReorderPoint,
    decimal? SafetyStock,
    int? LeadTimeDays,
    ProcurementType? ProcurementType,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    string? CustomerPartNo,
    string? DrawingNo,
    string? Revision,
    decimal? NetWeight,
    decimal? GrossWeight,
    decimal? Length,
    decimal? Width,
    decimal? Height,
    string? ImageUrl,
    string? ThumbnailUrl,
    bool IsActive,
    string? BarcodePattern,
    decimal? FixedPurchasePrice,
    string? TechnicalStandard,
    string? QuantityFormula,
    IReadOnlyList<ProductUoMConversionDto> UoMConversions);

public record ProductUoMConversionDto(
    int ConversionId,
    string UoMCode,
    decimal ToBaseFactor,
    string? Notes);
