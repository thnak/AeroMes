using FluentValidation;

namespace AeroMes.Application.Quality.InspectionResults.Commands.BulkRecordInspectionResults;

public class BulkRecordInspectionResultsValidator : AbstractValidator<BulkRecordInspectionResultsCommand>
{
    public BulkRecordInspectionResultsValidator()
    {
        RuleFor(x => x.InspectionOrderId).GreaterThan(0);
        RuleFor(x => x.RecordedBy).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Results).NotEmpty().WithMessage("At least one result item is required.");
        RuleForEach(x => x.Results).ChildRules(item =>
        {
            item.RuleFor(r => r.CharId).GreaterThan(0);
            item.RuleFor(r => r)
                .Must(r => r.MeasuredValue.HasValue || !string.IsNullOrWhiteSpace(r.AttributeResult))
                .WithName("Measurement")
                .WithMessage("At least one of MeasuredValue or AttributeResult must be provided.");
        });
    }
}
