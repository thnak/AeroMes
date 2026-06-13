using FluentValidation;

namespace AeroMes.Application.Traceability.Commands.ShipSerial;

public class ShipSerialValidator : AbstractValidator<ShipSerialCommand>
{
    public ShipSerialValidator()
    {
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ShipmentRef).NotEmpty().MaximumLength(100);
    }
}
