using AeroMes.Domain.Master;
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

        RuleFor(x => x.ScopeId)
            .NotEmpty().MaximumLength(50)
            .WithMessage("ScopeId là bắt buộc khi Scope là WorkCenter hoặc Machine.")
            .When(x => x.Scope is AlertScope.WorkCenter or AlertScope.Machine);

        RuleFor(x => x.ScopeId)
            .Empty()
            .WithMessage("ScopeId phải để trống khi Scope là Factory.")
            .When(x => x.Scope == AlertScope.Factory);
    }
}
