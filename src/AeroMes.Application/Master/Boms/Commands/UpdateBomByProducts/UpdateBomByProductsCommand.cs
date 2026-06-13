using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Boms.Commands.UpdateBomByProducts;

public record BomByProductEntry(string ByProductCode, decimal Quantity, string UoMCode, string? Notes);

public record UpdateBomByProductsCommand(
    string ProductCode,
    string Version,
    IReadOnlyList<BomByProductEntry> ByProducts,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
