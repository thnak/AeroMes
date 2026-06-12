using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.ProductAttributes.Commands.AssignAttributeToProduct;

public class AssignAttributeToProductValidator : AbstractValidator<AssignAttributeToProductCommand>
{
    public AssignAttributeToProductValidator(IProductRepository productRepo)
    {
        RuleFor(x => x.ProductCode)
            .NotEmpty()
            .MaximumLength(50)
            .MustAsync(async (code, ct) => await productRepo.ExistsAsync(code, ct))
            .WithMessage(x => $"Product '{x.ProductCode}' does not exist.");
    }
}
