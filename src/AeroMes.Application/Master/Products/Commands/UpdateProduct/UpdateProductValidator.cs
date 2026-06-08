using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Products.Commands.UpdateProduct;

public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator(IProductRepository repo)
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MustAsync(async (code, ct) => await repo.ExistsAsync(code, ct))
            .WithMessage(x => $"Product '{x.Code}' does not exist.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(150).WithMessage("Name must be at most 150 characters.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Unit is required.")
            .MaximumLength(20).WithMessage("Unit must be at most 20 characters.");

        RuleFor(x => x.BarcodePattern)
            .MaximumLength(200).WithMessage("BarcodePattern must be at most 200 characters.")
            .When(x => x.BarcodePattern is not null);

        RuleFor(x => x.UpdatedBy)
            .NotEmpty().WithMessage("UpdatedBy is required.");
    }
}
