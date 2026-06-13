using FluentValidation;

namespace AeroMes.Application.Quality.InspectionResults.Commands.RecordInspectionResult;

public class RecordInspectionResultValidator : AbstractValidator<RecordInspectionResultCommand>
{
    public RecordInspectionResultValidator()
    {
        RuleFor(x => x.InspectionOrderId).GreaterThan(0);
        RuleFor(x => x.CharId).GreaterThan(0);
        RuleFor(x => x.RecordedBy).NotEmpty().MaximumLength(100);
        RuleFor(x => x)
            .Must(x => x.MeasuredValue.HasValue || !string.IsNullOrWhiteSpace(x.AttributeResult))
            .WithName("Measurement")
            .WithMessage("At least one of MeasuredValue or AttributeResult must be provided.");
    }
}
