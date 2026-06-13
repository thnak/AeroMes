using FluentValidation;

namespace AeroMes.Application.Quality.InspectionPlans.Commands.AddCharacteristic;

public class AddCharacteristicValidator : AbstractValidator<AddCharacteristicCommand>
{
    private static readonly string[] ValidMeasurementTypes = ["VARIABLE", "ATTRIBUTE"];
    private static readonly string[] ValidSeverityLevels = ["CRITICAL", "MAJOR", "MINOR"];

    public AddCharacteristicValidator()
    {
        RuleFor(x => x.PlanId).GreaterThan(0);
        RuleFor(x => x.Sequence).GreaterThan(0);
        RuleFor(x => x.CharName).NotEmpty().MaximumLength(200);

        RuleFor(x => x.MeasurementType)
            .NotEmpty()
            .Must(t => ValidMeasurementTypes.Contains(t.ToUpperInvariant()))
            .WithMessage($"MeasurementType must be one of: {string.Join(", ", ValidMeasurementTypes)}.");

        RuleFor(x => x.SeverityLevel)
            .NotEmpty()
            .Must(s => ValidSeverityLevels.Contains(s.ToUpperInvariant()))
            .WithMessage($"SeverityLevel must be one of: {string.Join(", ", ValidSeverityLevels)}.");

        RuleFor(x => x.Unit).MaximumLength(30).When(x => x.Unit is not null);
        RuleFor(x => x.AttributeSpec).MaximumLength(200).When(x => x.AttributeSpec is not null);
        RuleFor(x => x.DefectCodeLink).MaximumLength(50).When(x => x.DefectCodeLink is not null);
        RuleFor(x => x.Notes).MaximumLength(255).When(x => x.Notes is not null);
    }
}
