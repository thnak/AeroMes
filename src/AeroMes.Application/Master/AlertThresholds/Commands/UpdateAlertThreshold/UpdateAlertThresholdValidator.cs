using FluentValidation;

namespace AeroMes.Application.Master.AlertThresholds.Commands.UpdateAlertThreshold;

public class UpdateAlertThresholdValidator : AbstractValidator<UpdateAlertThresholdCommand>
{
    public UpdateAlertThresholdValidator()
    {
        RuleFor(x => x.ThresholdId).GreaterThan(0);
        RuleFor(x => x.MetricKey).NotEmpty().MaximumLength(50);
        RuleFor(x => x.WarningLevel).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CriticalLevel)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(x => x.WarningLevel)
            .WithMessage("Critical level must be less than or equal to warning level.");
    }
}
