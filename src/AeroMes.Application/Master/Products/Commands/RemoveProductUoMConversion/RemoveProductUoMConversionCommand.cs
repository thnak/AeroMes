using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Products.Commands.RemoveProductUoMConversion;

public record RemoveProductUoMConversionCommand(
    string ProductCode,
    int ConversionId,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
