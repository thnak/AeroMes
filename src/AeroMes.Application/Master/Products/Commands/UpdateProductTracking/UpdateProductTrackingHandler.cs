using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.UpdateProductTracking;

public class UpdateProductTrackingHandler(
    IProductRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateProductTrackingCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateProductTrackingCommand cmd, CancellationToken ct)
    {
        try
        {
            var product = await repo.GetByCodeAsync(cmd.ProductCode, ct);
            if (product is null)
                return ValidationResult<Unit>.NotFound($"Product '{cmd.ProductCode}' not found.");

            product.UpdateTracking(
                cmd.TrackingMethod,
                cmd.SecondaryUnit,
                cmd.SecondaryUnitConversionFactor,
                cmd.ProductClass,
                cmd.UpdatedBy);

            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
