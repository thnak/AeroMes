using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Products.Commands.UpdateProduct;

public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator(
        IProductRepository repo,
        IUnitOfMeasureRepository uomRepo,
        IProductCategoryRepository categoryRepo)
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MustAsync(async (code, ct) => await repo.ExistsAsync(code, ct))
            .WithMessage(x => $"Product '{x.Code}' does not exist.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.BaseUoMCode)
            .NotEmpty()
            .MaximumLength(20)
            .MustAsync(async (code, ct) => await uomRepo.CodeExistsAsync(code, ct))
            .WithMessage(x => $"UoM '{x.BaseUoMCode}' not found.");

        RuleFor(x => x.PurchaseToBaseQty)
            .GreaterThan(0);

        RuleFor(x => x.ShelfLifeDays)
            .GreaterThan(0).When(x => x.ShelfLifeDays.HasValue);

        RuleFor(x => x.EffectiveTo)
            .GreaterThan(x => x.EffectiveFrom)
            .When(x => x.EffectiveFrom.HasValue && x.EffectiveTo.HasValue)
            .WithMessage("EffectiveTo must be after EffectiveFrom.");

        RuleFor(x => x.CategoryId)
            .MustAsync(async (id, ct) => id == null || await categoryRepo.IsActiveAsync(id.Value, ct))
            .WithMessage("Category does not exist or is inactive.")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.UpdatedBy)
            .NotEmpty();
    }
}
