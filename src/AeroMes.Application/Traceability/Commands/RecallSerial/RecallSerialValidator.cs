using FluentValidation;

namespace AeroMes.Application.Traceability.Commands.RecallSerial;

public class RecallSerialValidator : AbstractValidator<RecallSerialCommand>
{
    public RecallSerialValidator()
    {
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.RecallID).NotEmpty();
    }
}
