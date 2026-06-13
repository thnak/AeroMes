using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.AddAttributeValue;

public record AddAttributeValueCommand(
    int AttributeId,
    string Value,
    string? GroupName,
    int SortOrder,
    string? UpdatedBy) : ICommand<ValidationResult<int>>;
