using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.AddProductSpecification;

public class AddProductSpecificationHandler(
    IProductRepository repo,
    ISystemOptionsRepository optionsRepo,
    IUnitOfWork uow,
    IValidator<AddProductSpecificationCommand> validator) : ICommandHandler<AddProductSpecificationCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(AddProductSpecificationCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            await optionsRepo.EnsureModeAsync(MaterialManagementModes.SpecificationCode, ct);

            var product = await repo.GetByCodeWithSpecificationsAsync(cmd.ProductCode, ct)
                ?? throw new EntityNotFoundException("Product", cmd.ProductCode);

            var spec = product.AddSpecification(cmd.SpecCode, cmd.Description, cmd.CreatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(spec.SpecificationId);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<int>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
