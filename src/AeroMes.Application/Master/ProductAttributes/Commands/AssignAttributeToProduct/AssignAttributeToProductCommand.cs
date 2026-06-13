using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.AssignAttributeToProduct;

public record AssignAttributeToProductCommand(
    string ProductCode,
    int AttributeId,
    int? SelectedValueId,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
