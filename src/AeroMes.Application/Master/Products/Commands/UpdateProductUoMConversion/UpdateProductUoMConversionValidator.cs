using FluentValidation;

namespace AeroMes.Application.Master.Products.Commands.UpdateProductUoMConversion;

public class UpdateProductUoMConversionValidator : AbstractValidator<UpdateProductUoMConversionCommand>
{
    public UpdateProductUoMConversionValidator()
    {
        RuleFor(x => x.ToBaseFactor).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(255);
    }
}
