using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Products.Commands.CreateProduct;

public class CreateProductHandler(
    IProductRepository repo,
    IUnitOfWork uow,
    IValidator<CreateProductCommand> validator) : ICommandHandler<CreateProductCommand, ValidationResult<string>>
{
    public async Task<ValidationResult<string>> HandleAsync(CreateProductCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<string>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = Product.Create(
                cmd.Code, cmd.Name, cmd.BaseUoMCode, cmd.ItemType, cmd.CategoryId,
                cmd.BarcodePattern, cmd.LotControlled, cmd.SerialControlled, cmd.ShelfLifeDays,
                cmd.ProcurementType, cmd.CustomerPartNo, cmd.DrawingNo, cmd.Revision, cmd.CreatedBy);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<string>.Ok(entity.ProductCode);
        }        catch (DomainException ex)
        {
            return ValidationResult<string>.Failure(ex.Message);
        }
    }
}
