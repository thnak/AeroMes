using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateProductPickingConfig;

public class UpdateProductPickingConfigHandler(
    IProductRepository productRepo,
    IUnitOfWork uow,
    IValidator<UpdateProductPickingConfigCommand> validator)
    : ICommandHandler<UpdateProductPickingConfigCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateProductPickingConfigCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var product = await productRepo.GetByCodeAsync(cmd.ProductCode, ct);
            if (product is null)
                return ValidationResult<Unit>.NotFound($"Không tìm thấy sản phẩm '{cmd.ProductCode}'.");

            product.UpdatePickingConfig(cmd.PickingStrategy, cmd.MinShelfLifeDaysOnIssue, cmd.UpdatedBy ?? "system");
            await uow.SaveChangesAsync(ct);

            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
