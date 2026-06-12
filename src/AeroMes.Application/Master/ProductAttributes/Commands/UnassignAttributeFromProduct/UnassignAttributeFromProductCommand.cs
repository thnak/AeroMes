using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.UnassignAttributeFromProduct;

public record UnassignAttributeFromProductCommand(
    string ProductCode,
    int AttributeId,
    string? DeletedBy) : ICommand;
