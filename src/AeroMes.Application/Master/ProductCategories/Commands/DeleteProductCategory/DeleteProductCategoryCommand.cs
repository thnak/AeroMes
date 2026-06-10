using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductCategories.Commands.DeleteProductCategory;

public record DeleteProductCategoryCommand(int Id, string? DeletedBy) : ICommand;
