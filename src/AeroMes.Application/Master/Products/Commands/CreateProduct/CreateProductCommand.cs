using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Code,
    string Name,
    string BaseUoMCode,
    ItemType ItemType,
    int? CategoryId,
    string? BarcodePattern,
    bool LotControlled,
    bool SerialControlled,
    int? ShelfLifeDays,
    ProcurementType? ProcurementType,
    string? CustomerPartNo,
    string? DrawingNo,
    string? Revision,
    string? CreatedBy) : ICommand<ValidationResult<string>>;
