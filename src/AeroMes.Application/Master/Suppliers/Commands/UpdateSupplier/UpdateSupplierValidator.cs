using FluentValidation;

namespace AeroMes.Application.Master.Suppliers.Commands.UpdateSupplier;

public class UpdateSupplierValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Country).MaximumLength(50).When(x => x.Country != null);
        RuleFor(x => x.City).MaximumLength(100).When(x => x.City != null);
        RuleFor(x => x.Address).MaximumLength(300).When(x => x.Address != null);
        RuleFor(x => x.Phone).MaximumLength(30).When(x => x.Phone != null);
        RuleFor(x => x.Email).MaximumLength(100).EmailAddress().When(x => x.Email != null);
        RuleFor(x => x.TaxCode).MaximumLength(20).When(x => x.TaxCode != null);
    }
}
