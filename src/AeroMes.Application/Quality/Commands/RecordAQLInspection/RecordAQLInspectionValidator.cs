using FluentValidation;

namespace AeroMes.Application.Quality.Commands.RecordAQLInspection;

public class RecordAQLInspectionValidator : AbstractValidator<RecordAQLInspectionCommand>
{
    private static readonly string[] _validAQLLevels = ["1.0", "1.5", "2.5", "4.0"];
    private static readonly string[] _validInspLevels = ["I", "II", "III", "S1", "S2", "S3", "S4"];

    public RecordAQLInspectionValidator()
    {
        RuleFor(x => x.WOID).GreaterThan(0);
        RuleFor(x => x.AQLLevel).Must(v => _validAQLLevels.Contains(v))
            .WithMessage("AQLLevel must be one of: 1.0, 1.5, 2.5, 4.0");
        RuleFor(x => x.InspectionLevel).Must(v => _validInspLevels.Contains(v))
            .WithMessage("InspectionLevel must be one of: I, II, III, S1, S2, S3, S4");
        RuleFor(x => x.LotSize).GreaterThan(0);
        RuleFor(x => x.InspectorID).NotEmpty().MaximumLength(50);
        RuleForEach(x => x.Defects).ChildRules(d =>
        {
            d.RuleFor(x => x.DefectCode).NotEmpty().MaximumLength(30);
            d.RuleFor(x => x.Quantity).GreaterThan(0);
        });
    }
}
