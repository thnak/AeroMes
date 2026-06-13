using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.UpdateProductUoMConversion;

public record UpdateProductUoMConversionCommand(
    string ProductCode,
    int ConversionId,
    decimal ToBaseFactor,
    string? Notes,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
