using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.CreateProductVariant;

public class CreateProductVariantHandler(
    IProductRepository repo,
    ISystemOptionsRepository optionsRepo,
    IUnitOfWork uow,
    IValidator<CreateProductVariantCommand> validator) : ICommandHandler<CreateProductVariantCommand, ValidationResult<string>>
{
    public async Task<ValidationResult<string>> HandleAsync(CreateProductVariantCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<string>.Invalid(validation.ToErrorDictionary());

        try
        {
            await optionsRepo.EnsureModeAsync(MaterialManagementModes.VariantCode, ct);

            var parent = await repo.GetByCodeAsync(cmd.ParentProductCode, ct)
                ?? throw new EntityNotFoundException("Product", cmd.ParentProductCode);

            if (parent.ParentProductCode is not null)
                throw new DomainException(
                    $"'{parent.ProductCode}' đã là một biến thể — không thể tạo biến thể của biến thể.");

            // The variant inherits the master's classification and tracking setup;
            // it is a full product with its own BOM and inventory.
            var variant = Product.Create(
                cmd.Code, cmd.Name, parent.BaseUoMCode, parent.ItemType, parent.CategoryId,
                parent.BarcodePattern, parent.LotControlled, parent.SerialControlled,
                parent.ShelfLifeDays, parent.ProcurementType,
                parent.CustomerPartNo, parent.DrawingNo, parent.Revision,
                cmd.CreatedBy);
            variant.SetVariantParent(parent.ProductCode);

            await repo.AddAsync(variant, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<string>.Ok(variant.ProductCode);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<string>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<string>.Failure(ex.Message);
        }
    }
}
