using FluentValidation;

namespace AeroMes.Application.Wms.Commands.CreateCycleCountPlan;

public class CreateCycleCountPlanValidator : AbstractValidator<CreateCycleCountPlanCommand>
{
    public CreateCycleCountPlanValidator()
    {
        RuleFor(x => x.ScheduledDate)
            .NotEmpty().WithMessage("Ngày kiểm kê là bắt buộc.");
    }
}
