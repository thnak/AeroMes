using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.RemoveProductUoMConversion;

public class RemoveProductUoMConversionHandler(
    IProductRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveProductUoMConversionCommand>
{
    public async Task HandleAsync(RemoveProductUoMConversionCommand cmd, CancellationToken ct)
    {
        var product = await repo.GetByCodeWithConversionsAsync(cmd.ProductCode, ct)
            ?? throw new EntityNotFoundException("Product", cmd.ProductCode);

        product.RemoveUoMConversion(cmd.ConversionId, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
