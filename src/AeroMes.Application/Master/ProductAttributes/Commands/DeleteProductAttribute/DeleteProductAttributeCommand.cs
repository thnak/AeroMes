using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.ProductAttributes.Commands.DeleteProductAttribute;

public record DeleteProductAttributeCommand(int AttributeId, string? DeletedBy) : ICommand<ValidationResult<Unit>>;
