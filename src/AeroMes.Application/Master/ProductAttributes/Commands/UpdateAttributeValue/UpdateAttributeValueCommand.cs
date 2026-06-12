using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.UpdateAttributeValue;

public record UpdateAttributeValueCommand(
    int AttributeId,
    int ValueId,
    string Value,
    string? GroupName,
    int SortOrder,
    string? UpdatedBy) : ICommand;
