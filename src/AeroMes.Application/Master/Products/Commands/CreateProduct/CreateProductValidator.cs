using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Products.Commands.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator(
        IProductRepository repo,
        IUnitOfMeasureRepository uomRepo,
        IProductCategoryRepository categoryRepo)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("Code must be uppercase alphanumeric with dashes only.")
            .MustAsync(async (code, ct) => !await repo.ExistsAsync(code, ct))
            .WithMessage(x => $"Product code '{x.Code}' already exists.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.BaseUoMCode)
            .NotEmpty()
            .MaximumLength(20)
            .MustAsync(async (code, ct) => await uomRepo.CodeExistsAsync(code, ct))
            .WithMessage(x => $"UoM '{x.BaseUoMCode}' not found.");

        RuleFor(x => x.ShelfLifeDays)
            .GreaterThan(0).When(x => x.ShelfLifeDays.HasValue);

        RuleFor(x => x.BarcodePattern)
            .MaximumLength(200).When(x => x.BarcodePattern is not null);

        RuleFor(x => x.CategoryId)
            .MustAsync(async (id, ct) => id == null || await categoryRepo.IsActiveAsync(id.Value, ct))
            .WithMessage("Category does not exist or is inactive.")
            .When(x => x.CategoryId.HasValue);
    }
}
