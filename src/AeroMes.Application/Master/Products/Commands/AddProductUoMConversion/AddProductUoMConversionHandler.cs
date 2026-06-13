using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Products.Commands.AddProductUoMConversion;

public class AddProductUoMConversionHandler(
    IProductRepository repo,
    IUnitOfWork uow,
    IValidator<AddProductUoMConversionCommand> validator) : ICommandHandler<AddProductUoMConversionCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(AddProductUoMConversionCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var product = await repo.GetByCodeWithConversionsAsync(cmd.ProductCode, ct);
            if (product is null) return ValidationResult<int>.NotFound($"Product '{cmd.ProductCode}' was not found.");

            var conversion = product.AddUoMConversion(cmd.UoMCode, cmd.ToBaseFactor, cmd.Notes, cmd.CreatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(conversion.ConversionId);
        }        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
