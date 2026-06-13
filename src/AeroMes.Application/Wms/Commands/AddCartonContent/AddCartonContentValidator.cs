using FluentValidation;

namespace AeroMes.Application.Wms.Commands.AddCartonContent;

public class AddCartonContentValidator : AbstractValidator<AddCartonContentCommand>
{
    public AddCartonContentValidator()
    {
        RuleFor(x => x.CartonId).GreaterThan(0);
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LotNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PackedQty).GreaterThan(0);
    }
}
