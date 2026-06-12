using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductCategories.Commands.CreateProductCategory;

public record CreateProductCategoryCommand(
    int? ParentId,
    string Code,
    string Name,
    string? Description,
    decimal? StandardProductionTime,
    string? Color,
    string? CreatedBy) : ICommand<int>;
