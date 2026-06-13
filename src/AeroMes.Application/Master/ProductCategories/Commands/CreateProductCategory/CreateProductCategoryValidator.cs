using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.ProductCategories.Commands.CreateProductCategory;

public class CreateProductCategoryValidator : AbstractValidator<CreateProductCategoryCommand>
{
    public CreateProductCategoryValidator(IProductCategoryRepository repo)
    {
        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(30)
            .MustAsync(async (code, ct) => !await repo.CodeExistsAsync(code, ct))
            .WithMessage(x => $"Category code '{x.Code}' already exists.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ParentId)
            .Cascade(CascadeMode.Stop)
            .MustAsync(async (id, ct) => id == null || await repo.GetByIdAsync(id.Value, ct) != null)
            .WithMessage("Parent category not found.")
            .When(x => x.ParentId.HasValue);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.StandardProductionTime)
            .GreaterThan(0).When(x => x.StandardProductionTime.HasValue);

        RuleFor(x => x.Color)
            .MaximumLength(20);
    }
}
