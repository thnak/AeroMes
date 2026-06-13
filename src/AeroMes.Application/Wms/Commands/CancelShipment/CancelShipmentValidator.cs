using FluentValidation;

namespace AeroMes.Application.Wms.Commands.CancelShipment;

public class CancelShipmentValidator : AbstractValidator<CancelShipmentCommand>
{
    public CancelShipmentValidator()
    {
        RuleFor(x => x.ShipmentId).GreaterThan(0);
    }
}
