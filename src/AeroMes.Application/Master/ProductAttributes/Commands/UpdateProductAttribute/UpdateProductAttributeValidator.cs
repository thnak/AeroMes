using FluentValidation;

namespace AeroMes.Application.Master.ProductAttributes.Commands.UpdateProductAttribute;

public class UpdateProductAttributeValidator : AbstractValidator<UpdateProductAttributeCommand>
{
    public UpdateProductAttributeValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
