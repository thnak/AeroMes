using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.AddProductUoMConversion;

public record AddProductUoMConversionCommand(
    string ProductCode,
    string UoMCode,
    decimal ToBaseFactor,
    string? Notes,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
