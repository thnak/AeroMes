using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductCategories.Commands.UpdateProductCategory;

public record UpdateProductCategoryCommand(
    int Id,
    int? ParentId,
    string Name,
    bool IsActive,
    string UpdatedBy) : ICommand;
