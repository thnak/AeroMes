using FluentValidation;

namespace AeroMes.Application.Master.ProductAttributes.Commands.UpdateAttributeValue;

public class UpdateAttributeValueValidator : AbstractValidator<UpdateAttributeValueCommand>
{
    public UpdateAttributeValueValidator()
    {
        RuleFor(x => x.Value).NotEmpty().MaximumLength(100);
        RuleFor(x => x.GroupName).MaximumLength(100);
    }
}
