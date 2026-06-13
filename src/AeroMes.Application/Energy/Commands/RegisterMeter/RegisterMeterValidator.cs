using FluentValidation;

namespace AeroMes.Application.Energy.Commands.RegisterMeter;

public class RegisterMeterValidator : AbstractValidator<RegisterMeterCommand>
{
    public RegisterMeterValidator()
    {
        RuleFor(x => x.MeterCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.MeterName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20);
    }
}
