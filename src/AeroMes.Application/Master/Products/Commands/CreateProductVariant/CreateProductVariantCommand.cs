using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.CreateProductVariant;

public record CreateProductVariantCommand(
    string ParentProductCode,
    string Code,
    string Name,
    string? CreatedBy) : ICommand<ValidationResult<string>>;
