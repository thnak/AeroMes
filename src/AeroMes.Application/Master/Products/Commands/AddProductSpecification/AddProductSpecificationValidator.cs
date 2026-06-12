using FluentValidation;

namespace AeroMes.Application.Master.Products.Commands.AddProductSpecification;

public class AddProductSpecificationValidator : AbstractValidator<AddProductSpecificationCommand>
{
    public AddProductSpecificationValidator()
    {
        RuleFor(x => x.SpecCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(255);
    }
}
