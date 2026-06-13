using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Products.Commands.RemoveProductUoMConversion;

public class RemoveProductUoMConversionHandler(
    IProductRepository repo,
    IUnitOfWork uow) : ICommandHandler<RemoveProductUoMConversionCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(RemoveProductUoMConversionCommand cmd, CancellationToken ct)
    {
        var product = await repo.GetByCodeWithConversionsAsync(cmd.ProductCode, ct);
        if (product is null) return ValidationResult<Unit>.NotFound($"Product '{cmd.ProductCode}' was not found.");

        product.RemoveUoMConversion(cmd.ConversionId, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
