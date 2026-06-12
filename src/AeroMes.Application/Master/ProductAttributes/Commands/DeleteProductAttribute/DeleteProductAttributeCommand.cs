using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.DeleteProductAttribute;

public record DeleteProductAttributeCommand(int AttributeId, string? DeletedBy) : ICommand;
