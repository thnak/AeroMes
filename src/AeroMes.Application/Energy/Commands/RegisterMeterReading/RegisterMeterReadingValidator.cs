using FluentValidation;

namespace AeroMes.Application.Energy.Commands.RegisterMeterReading;

public class RegisterMeterReadingValidator : AbstractValidator<RegisterMeterReadingCommand>
{
    public RegisterMeterReadingValidator()
    {
        RuleFor(x => x.MeterID).GreaterThan(0);
        RuleFor(x => x.ReadingValue).GreaterThanOrEqualTo(0);
    }
}
