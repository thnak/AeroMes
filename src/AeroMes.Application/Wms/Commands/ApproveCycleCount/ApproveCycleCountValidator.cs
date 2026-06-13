using FluentValidation;

namespace AeroMes.Application.Wms.Commands.ApproveCycleCount;

public class ApproveCycleCountValidator : AbstractValidator<ApproveCycleCountCommand>
{
    public ApproveCycleCountValidator()
    {
        RuleFor(x => x.VarianceThresholdPct)
            .GreaterThanOrEqualTo(0).WithMessage("Ngưỡng chênh lệch không được âm.")
            .LessThanOrEqualTo(100).WithMessage("Ngưỡng chênh lệch không được vượt quá 100%.");
    }
}
