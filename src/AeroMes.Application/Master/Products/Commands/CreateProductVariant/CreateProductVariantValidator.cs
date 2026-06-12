using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Products.Commands.CreateProductVariant;

public class CreateProductVariantValidator : AbstractValidator<CreateProductVariantCommand>
{
    public CreateProductVariantValidator(IProductRepository repo)
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("Code must be uppercase alphanumeric with dashes only.")
            .MustAsync(async (code, ct) => !await repo.ExistsAsync(code, ct))
            .WithMessage(x => $"Product code '{x.Code}' already exists.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}
