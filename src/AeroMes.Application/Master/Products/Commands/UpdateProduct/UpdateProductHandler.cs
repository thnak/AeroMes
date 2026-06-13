using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.UpdateProduct;

public class UpdateProductHandler(
    IProductRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateProductCommand> validator) : ICommandHandler<UpdateProductCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateProductCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByCodeAsync(cmd.Code, ct)
                ?? throw new EntityNotFoundException("Product", cmd.Code);
            entity.UpdateDetails(
                cmd.Name, cmd.BaseUoMCode, cmd.PurchaseUoMCode, cmd.PurchaseToBaseQty,
                cmd.ItemType, cmd.CategoryId, cmd.BarcodePattern,
                cmd.LotControlled, cmd.SerialControlled, cmd.ShelfLifeDays,
                cmd.ReorderPoint, cmd.SafetyStock, cmd.LeadTimeDays, cmd.ProcurementType,
                cmd.EffectiveFrom, cmd.EffectiveTo,
                cmd.CustomerPartNo, cmd.DrawingNo, cmd.Revision,
                cmd.NetWeight, cmd.GrossWeight, cmd.Length, cmd.Width, cmd.Height,
                cmd.ImageUrl, cmd.ThumbnailUrl,
                cmd.FixedPurchasePrice, cmd.TechnicalStandard, cmd.QuantityFormula,
                cmd.UpdatedBy);
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
