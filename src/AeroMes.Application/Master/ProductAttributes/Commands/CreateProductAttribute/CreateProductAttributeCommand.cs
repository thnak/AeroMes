using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.CreateProductAttribute;

public record CreateProductAttributeCommand(
    string Code,
    string Name,
    IReadOnlyList<AttributeValueEntry> Values,
    string? CreatedBy) : ICommand<ValidationResult<int>>;

public record AttributeValueEntry(string Value, string? GroupName, int SortOrder);
