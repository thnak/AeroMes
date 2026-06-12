using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Commands.CreateProductAttribute;

public record CreateProductAttributeCommand(
    string Code,
    string Name,
    IReadOnlyList<AttributeValueEntry> Values,
    string? CreatedBy) : ICommand<int>;

public record AttributeValueEntry(string Value, string? GroupName, int SortOrder);
