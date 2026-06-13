using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.UpdateProductUoMConversion;

public class UpdateProductUoMConversionHandler(
    IProductRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateProductUoMConversionCommand> validator) : ICommandHandler<UpdateProductUoMConversionCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateProductUoMConversionCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var product = await repo.GetByCodeWithConversionsAsync(cmd.ProductCode, ct);
            if (product is null)
                return ValidationResult<Unit>.NotFound($"Product '{cmd.ProductCode}' not found.");

            product.UpdateUoMConversion(cmd.ConversionId, cmd.ToBaseFactor, cmd.Notes, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<Unit>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
