using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.ProductCategories.Commands.DeleteProductCategory;

public record DeleteProductCategoryCommand(int Id, string? DeletedBy) : ICommand<ValidationResult<Unit>>;
