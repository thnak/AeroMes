using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.UpdateProductSpecification;

public record UpdateProductSpecificationCommand(
    string ProductCode,
    int SpecificationId,
    string? Description,
    bool IsActive,
    string? UpdatedBy) : ICommand;
