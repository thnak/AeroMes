using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.ProductAttributes.Commands.UnassignAttributeFromProduct;

public record UnassignAttributeFromProductCommand(
    string ProductCode,
    int AttributeId,
    string? DeletedBy) : ICommand<ValidationResult<Unit>>;
