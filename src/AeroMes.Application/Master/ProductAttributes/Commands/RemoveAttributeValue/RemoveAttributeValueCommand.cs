using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.ProductAttributes.Commands.RemoveAttributeValue;

public record RemoveAttributeValueCommand(int AttributeId, int ValueId, string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
