using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.UpdateProductUoMConversion;

public class UpdateProductUoMConversionHandler(
    IProductRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateProductUoMConversionCommand>
{
    public async Task HandleAsync(UpdateProductUoMConversionCommand cmd, CancellationToken ct)
    {
        var product = await repo.GetByCodeWithConversionsAsync(cmd.ProductCode, ct)
            ?? throw new EntityNotFoundException("Product", cmd.ProductCode);

        product.UpdateUoMConversion(cmd.ConversionId, cmd.ToBaseFactor, cmd.Notes, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
