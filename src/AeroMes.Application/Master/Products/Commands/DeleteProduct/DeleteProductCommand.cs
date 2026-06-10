using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.DeleteProduct;

public record DeleteProductCommand(string Code) : ICommand;
