using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    string Code,
    string Name,
    string Unit,
    bool IsFinishedGood,
    string? BarcodePattern,
    string UpdatedBy) : ICommand;
