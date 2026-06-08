using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Products.Commands.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator(IProductRepository repo)
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(30).WithMessage("Code must be at most 30 characters.")
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("Code may only contain letters, digits, hyphens, and underscores.")
            .MustAsync(async (code, ct) => !await repo.ExistsAsync(code, ct))
            .WithMessage(x => $"Product code '{x.Code}' already exists.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(150).WithMessage("Name must be at most 150 characters.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Unit is required.")
            .MaximumLength(20).WithMessage("Unit must be at most 20 characters.");

        RuleFor(x => x.BarcodePattern)
            .MaximumLength(200).WithMessage("BarcodePattern must be at most 200 characters.")
            .When(x => x.BarcodePattern is not null);
    }
}
