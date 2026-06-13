using AeroMes.Domain.Quality.Repositories;
using FluentValidation;

namespace AeroMes.Application.Quality.InspectionPlans.Commands.CreateInspectionPlan;

public class CreateInspectionPlanValidator : AbstractValidator<CreateInspectionPlanCommand>
{
    private static readonly string[] ValidSamplingMethods = ["FULL", "AQL", "FIXED_N"];
    private static readonly string[] ValidInspectionTypes = ["DIMENSIONAL", "VISUAL", "FUNCTIONAL", "COMBINED"];

    public CreateInspectionPlanValidator(IInspectionPlanRepository repo)
    {
        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().MaximumLength(50)
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("Code may only contain letters, digits, hyphens, and underscores.")
            .MustAsync(async (code, ct) => !await repo.ExistsByCodeAsync(code, ct))
            .WithMessage(x => $"Inspection plan '{x.Code}' already exists.");

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
