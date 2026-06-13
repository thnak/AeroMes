using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductCategories.Commands.UpdateProductCategory;

public record UpdateProductCategoryCommand(
    int Id,
    int? ParentId,
    string Name,
    string? Description,
    decimal? StandardProductionTime,
    string? Color,
    bool IsActive,
    string UpdatedBy) : ICommand<ValidationResult<Unit>>;
