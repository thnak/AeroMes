using FluentValidation;

namespace AeroMes.Application.Master.WorkOrderAutoRules.Commands.UpsertWorkOrderAutoRules;

public class UpsertWorkOrderAutoRulesValidator : AbstractValidator<UpsertWorkOrderAutoRulesCommand>
{
    public UpsertWorkOrderAutoRulesValidator()
    {
        RuleFor(x => x.MaxConcurrentJobs)
            .GreaterThan(0).WithMessage("MaxConcurrentJobs must be at least 1.")
            .LessThanOrEqualTo(50).WithMessage("MaxConcurrentJobs may not exceed 50.");
    }
}
