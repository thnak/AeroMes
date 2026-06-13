using FluentValidation;

namespace AeroMes.Application.Wms.Commands.AddShipmentLine;

public class AddShipmentLineValidator : AbstractValidator<AddShipmentLineCommand>
{
    public AddShipmentLineValidator()
    {
        RuleFor(x => x.ShipmentId).GreaterThan(0);
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.OrderedQty).GreaterThan(0);
    }
}
