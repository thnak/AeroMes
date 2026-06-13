using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Products.Commands.DeleteProduct;

public record DeleteProductCommand(string Code, string? DeletedBy = null) : ICommand<ValidationResult<Unit>>;
