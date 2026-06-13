using FluentValidation;

namespace AeroMes.Application.Iot.Signals.Commands.UpdateSignal;

public class UpdateSignalValidator : AbstractValidator<UpdateSignalCommand>
{
    public UpdateSignalValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.SourceAddress).NotEmpty();
        RuleFor(x => x.Scale).NotEqual(0).WithMessage("Scale cannot be zero.");
    }
}
