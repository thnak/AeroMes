using FluentValidation;

namespace AeroMes.Application.Master.AlertThresholds.Commands.CreateAlertThreshold;

public class CreateAlertThresholdValidator : AbstractValidator<CreateAlertThresholdCommand>
{
    public CreateAlertThresholdValidator()
    {
        RuleFor(x => x.MetricKey).NotEmpty().MaximumLength(50);
        RuleFor(x => x.WarningLevel).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CriticalLevel)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(x => x.WarningLevel)
            .WithMessage("Critical level must be less than or equal to warning level.");
    }
}
