using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.UpdateProductCustomAttributes;

public class UpdateProductCustomAttributesHandler(
    IProductRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateProductCustomAttributesCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateProductCustomAttributesCommand cmd, CancellationToken ct)
    {
        var product = await repo.GetByCodeAsync(cmd.ProductCode, ct);
        if (product is null)
            return ValidationResult<Unit>.NotFound($"Product '{cmd.ProductCode}' not found.");

        product.UpdateCustomAttributes(cmd.CustomAttributesJson, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
