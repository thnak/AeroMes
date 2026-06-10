using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.ProductCategories.Commands.UpdateProductCategory;

public class UpdateProductCategoryValidator : AbstractValidator<UpdateProductCategoryCommand>
{
    public UpdateProductCategoryValidator(IProductCategoryRepository repo)
    {
        RuleFor(x => x.Id)
            .MustAsync(async (id, ct) => await repo.GetByIdAsync(id, ct) != null)
            .WithMessage(x => $"ProductCategory '{x.Id}' does not exist.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.UpdatedBy)
            .NotEmpty();
    }
}
