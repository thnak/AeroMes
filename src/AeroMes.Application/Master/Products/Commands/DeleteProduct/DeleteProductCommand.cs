using MediatR;

namespace AeroMes.Application.Master.Products.Commands.DeleteProduct;

public record DeleteProductCommand(string Code) : IRequest;
