using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.UpdateProductCustomAttributes;

public record UpdateProductCustomAttributesCommand(
    string ProductCode,
    string? CustomAttributesJson,
    string UpdatedBy) : ICommand<ValidationResult<Unit>>;
