using FluentValidation;

namespace AeroMes.Application.Quality.Commands.RecordInlineInspection;

public class RecordInlineInspectionValidator : AbstractValidator<RecordInlineInspectionCommand>
{
    public RecordInlineInspectionValidator()
    {
        RuleFor(x => x.WOID).GreaterThan(0);
        RuleFor(x => x.WorkCenterID).GreaterThan(0);
        RuleFor(x => x.StyleCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.InspectorID).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ShiftCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.SampleSize).GreaterThan(0);
        RuleFor(x => x.DHU_Target).GreaterThan(0);
        RuleForEach(x => x.Defects).ChildRules(d =>
        {
            d.RuleFor(x => x.DefectCode).NotEmpty().MaximumLength(30);
            d.RuleFor(x => x.Quantity).GreaterThan(0);
        });
    }
}
