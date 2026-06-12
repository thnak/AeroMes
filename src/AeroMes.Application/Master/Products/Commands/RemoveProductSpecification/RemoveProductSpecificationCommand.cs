using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.RemoveProductSpecification;

public record RemoveProductSpecificationCommand(
    string ProductCode,
    int SpecificationId,
    string? UpdatedBy) : ICommand;
