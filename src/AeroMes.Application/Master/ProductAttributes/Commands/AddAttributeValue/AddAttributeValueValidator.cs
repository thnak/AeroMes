using FluentValidation;

namespace AeroMes.Application.Master.ProductAttributes.Commands.AddAttributeValue;

public class AddAttributeValueValidator : AbstractValidator<AddAttributeValueCommand>
{
    public AddAttributeValueValidator()
    {
        RuleFor(x => x.Value).NotEmpty().MaximumLength(100);
        RuleFor(x => x.GroupName).MaximumLength(100);
    }
}
