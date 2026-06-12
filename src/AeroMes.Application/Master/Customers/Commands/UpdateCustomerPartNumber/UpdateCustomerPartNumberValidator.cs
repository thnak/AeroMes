using FluentValidation;

namespace AeroMes.Application.Master.Customers.Commands.UpdateCustomerPartNumber;

public class UpdateCustomerPartNumberValidator : AbstractValidator<UpdateCustomerPartNumberCommand>
{
    public UpdateCustomerPartNumberValidator()
    {
        RuleFor(x => x.CustomerPartNumberId).GreaterThan(0);
        RuleFor(x => x.Description).MaximumLength(300).When(x => x.Description != null);
        RuleFor(x => x.DrawingReference).MaximumLength(100).When(x => x.DrawingReference != null);
        RuleFor(x => x.Revision).MaximumLength(20).When(x => x.Revision != null);
    }
}
