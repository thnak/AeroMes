using FluentValidation;

namespace AeroMes.Application.Master.Products.Commands.UpdateProductSpecification;

public class UpdateProductSpecificationValidator : AbstractValidator<UpdateProductSpecificationCommand>
{
    public UpdateProductSpecificationValidator()
    {
        RuleFor(x => x.Description).MaximumLength(255);
    }
}
