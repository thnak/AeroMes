using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Customers.Commands.CreateCustomer;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator(ICustomerRepository repo)
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TaxId).MaximumLength(50).When(x => x.TaxId != null);
        RuleFor(x => x.Country).MaximumLength(80).When(x => x.Country != null);
        RuleFor(x => x.Address).MaximumLength(300).When(x => x.Address != null);
        RuleFor(x => x.ShippingAddress).MaximumLength(300).When(x => x.ShippingAddress != null);
        RuleFor(x => x.ContactName).MaximumLength(150).When(x => x.ContactName != null);
        RuleFor(x => x.ContactPhone).MaximumLength(30).When(x => x.ContactPhone != null);
        RuleFor(x => x.ContactEmail).MaximumLength(150).EmailAddress().When(x => x.ContactEmail != null);
        RuleFor(x => x.CreditTermsDays).InclusiveBetween(0, 365);
        RuleFor(x => x.Currency).Length(3).When(x => !string.IsNullOrWhiteSpace(x.Currency));
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes != null);
        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .MustAsync(async (code, ct) => !await repo.CodeExistsAsync(code, ct))
            .WithMessage("Customer code already exists.");
    }
}
