using FluentValidation;

namespace AeroMes.Application.Production.Commands.CompleteCutting;

public class CompleteCuttingValidator : AbstractValidator<CompleteCuttingCommand>
{
    public CompleteCuttingValidator()
    {
        RuleFor(x => x.CutOrderID).GreaterThan(0);
        RuleFor(x => x.ActualFabricMeters).GreaterThan(0);
        RuleFor(x => x.MarkerEfficiencyPct).InclusiveBetween(0, 100);
        RuleFor(x => x.Lines).NotEmpty();
        RuleFor(x => x.BundleSize).GreaterThan(0);
    }
}
