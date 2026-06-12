using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.RemoveProductUoMConversion;

public record RemoveProductUoMConversionCommand(
    string ProductCode,
    int ConversionId,
    string? UpdatedBy) : ICommand;
