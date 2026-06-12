using FluentValidation;

namespace AeroMes.Application.Master.OrgUnits.Commands.SyncOrgUnits;

public class SyncOrgUnitsValidator : AbstractValidator<SyncOrgUnitsCommand>
{
    public SyncOrgUnitsValidator()
    {
        // An empty snapshot would deactivate the entire hierarchy — refuse it.
        RuleFor(x => x.Units).NotEmpty();

        RuleFor(x => x.Units)
            .Must(units => units
                .Select(u => u.UnitCode.Trim().ToUpperInvariant())
                .Distinct()
                .Count() == units.Count)
            .WithMessage("Snapshot contains duplicate unit codes.");

        RuleForEach(x => x.Units).ChildRules(u =>
        {
            u.RuleFor(x => x.UnitCode).NotEmpty().MaximumLength(50);
            u.RuleFor(x => x.UnitName).NotEmpty().MaximumLength(200);
            u.RuleFor(x => x.ParentUnitCode).MaximumLength(50);
            u.RuleFor(x => x.SourceSystemId).NotEmpty().MaximumLength(100);
            u.RuleFor(x => x.UnitType).IsInEnum();
        });
    }
}
