using FluentValidation;

namespace AeroMes.Application.Master.DowntimeReasonCodes.Commands.UpdateDowntimeReasonCode;

public class UpdateDowntimeReasonCodeValidator : AbstractValidator<UpdateDowntimeReasonCodeCommand>
{
    public UpdateDowntimeReasonCodeValidator()
    {
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SlaMinutes)
            .GreaterThan(0).WithMessage("SLA minutes must be greater than zero.")
            .When(x => x.SlaMinutes.HasValue);
    }
}
