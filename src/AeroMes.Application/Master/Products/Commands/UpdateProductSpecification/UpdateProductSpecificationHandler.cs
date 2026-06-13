using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Products.Commands.UpdateProductSpecification;

public class UpdateProductSpecificationHandler(
    IProductRepository repo,
    ISystemOptionsRepository optionsRepo,
    IUnitOfWork uow,
    IValidator<UpdateProductSpecificationCommand> validator) : ICommandHandler<UpdateProductSpecificationCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateProductSpecificationCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            await optionsRepo.EnsureModeAsync(MaterialManagementModes.SpecificationCode, ct);

            var product = await repo.GetByCodeWithSpecificationsAsync(cmd.ProductCode, ct);
            if (product is null) return ValidationResult<Unit>.NotFound($"Product '{cmd.ProductCode}' was not found.");

            product.UpdateSpecification(cmd.SpecificationId, cmd.Description, cmd.IsActive, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);

            return ValidationResult<Unit>.Ok(Unit.Value);
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
