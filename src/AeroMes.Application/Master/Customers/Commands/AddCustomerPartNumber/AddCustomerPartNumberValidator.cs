using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Customers.Commands.AddCustomerPartNumber;

public class AddCustomerPartNumberValidator : AbstractValidator<AddCustomerPartNumberCommand>
{
    public AddCustomerPartNumberValidator(IProductRepository productRepo)
    {
        RuleFor(x => x.CustomerPartNo).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(300).When(x => x.Description != null);
        RuleFor(x => x.DrawingReference).MaximumLength(100).When(x => x.DrawingReference != null);
        RuleFor(x => x.Revision).MaximumLength(20).When(x => x.Revision != null);
        RuleFor(x => x.ProductCode)
            .Cascade(CascadeMode.Stop)
            .MustAsync(async (code, ct) => await productRepo.ExistsAsync(code, ct))
            .WithMessage("Product does not exist.");
    }
}
