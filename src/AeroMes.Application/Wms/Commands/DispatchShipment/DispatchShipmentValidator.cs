using FluentValidation;

namespace AeroMes.Application.Wms.Commands.DispatchShipment;

public class DispatchShipmentValidator : AbstractValidator<DispatchShipmentCommand>
{
    public DispatchShipmentValidator()
    {
        RuleFor(x => x.ShipmentId).GreaterThan(0);
        RuleFor(x => x.CarrierName).MaximumLength(100).When(x => x.CarrierName is not null);
        RuleFor(x => x.TrackingNumber).MaximumLength(100).When(x => x.TrackingNumber is not null);
    }
}
