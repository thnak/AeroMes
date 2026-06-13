using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    string Code,
    string Name,
    string BaseUoMCode,
    string? PurchaseUoMCode,
    decimal PurchaseToBaseQty,
    ItemType ItemType,
    int? CategoryId,
    string? BarcodePattern,
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
    decimal? FixedPurchasePrice,
    string? TechnicalStandard,
    string? QuantityFormula,
    string UpdatedBy) : ICommand<ValidationResult<Unit>>;
