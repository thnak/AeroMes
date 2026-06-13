using FluentValidation;

namespace AeroMes.Application.Wms.Commands.AddRmaLine;

public class AddRmaLineValidator : AbstractValidator<AddRmaLineCommand>
{
    public AddRmaLineValidator()
    {
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LotNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ReturnQty).GreaterThan(0);
    }
}
