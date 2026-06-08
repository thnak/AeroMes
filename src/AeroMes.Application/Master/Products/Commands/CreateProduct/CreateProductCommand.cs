using MediatR;

namespace AeroMes.Application.Master.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Code,
    string Name,
    string Unit,
    bool IsFinishedGood,
    string? BarcodePattern,
    string? CreatedBy) : IRequest<string>;
