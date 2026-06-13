using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

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
            var entity = await repo.GetByCodeAsync(cmd.Code, ct);
            if (entity is null) return ValidationResult<Unit>.NotFound($"Product '{cmd.Code}' was not found.");
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
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
