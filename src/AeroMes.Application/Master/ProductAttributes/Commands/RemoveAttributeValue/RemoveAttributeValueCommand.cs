using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.RemoveAttributeValue;

public record RemoveAttributeValueCommand(int AttributeId, int ValueId, string? UpdatedBy) : ICommand;
