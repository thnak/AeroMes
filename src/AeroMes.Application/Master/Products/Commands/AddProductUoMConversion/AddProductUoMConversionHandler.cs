using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.AddProductUoMConversion;

public class AddProductUoMConversionHandler(
    IProductRepository repo,
    IUnitOfWork uow) : ICommandHandler<AddProductUoMConversionCommand, int>
{
    public async Task<int> HandleAsync(AddProductUoMConversionCommand cmd, CancellationToken ct)
    {
        var product = await repo.GetByCodeWithConversionsAsync(cmd.ProductCode, ct)
            ?? throw new EntityNotFoundException("Product", cmd.ProductCode);

        var conversion = product.AddUoMConversion(cmd.UoMCode, cmd.ToBaseFactor, cmd.Notes, cmd.CreatedBy);
        await uow.SaveChangesAsync(ct);
        return conversion.ConversionId;
    }
}
