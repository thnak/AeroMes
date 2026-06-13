using FluentValidation;

namespace AeroMes.Application.Quality.InspectionPlans.Commands.UpdateInspectionPlan;

public class UpdateInspectionPlanValidator : AbstractValidator<UpdateInspectionPlanCommand>
{
    private static readonly string[] ValidSamplingMethods = ["FULL", "AQL", "FIXED_N"];
    private static readonly string[] ValidInspectionTypes = ["DIMENSIONAL", "VISUAL", "FUNCTIONAL", "COMBINED"];

    public UpdateInspectionPlanValidator()
    {
        RuleFor(x => x.PlanId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RoutingStepId).GreaterThan(0);
        RuleFor(x => x.ProductCode).MaximumLength(50).When(x => x.ProductCode is not null);

        RuleFor(x => x.SamplingMethod)
            .NotEmpty()
            .Must(m => ValidSamplingMethods.Contains(m.ToUpperInvariant()))
            .WithMessage($"SamplingMethod must be one of: {string.Join(", ", ValidSamplingMethods)}.");

        RuleFor(x => x.InspectionType)
            .NotEmpty()
            .Must(t => ValidInspectionTypes.Contains(t.ToUpperInvariant()))
            .WithMessage($"InspectionType must be one of: {string.Join(", ", ValidInspectionTypes)}.");

        RuleFor(x => x.AcceptNumber).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RejectNumber).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes is not null);
    }
}
