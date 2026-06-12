using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.UpdateProductAttribute;

public record UpdateProductAttributeCommand(
    int AttributeId,
    string Name,
    bool IsActive,
    string? UpdatedBy) : ICommand;
