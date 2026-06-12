using FluentValidation;

namespace AeroMes.Application.Master.Tools.Commands.RecordToolMaintenance;

public class RecordToolMaintenanceValidator : AbstractValidator<RecordToolMaintenanceCommand>
{
    public RecordToolMaintenanceValidator()
    {
        RuleFor(x => x.ToolCode).NotEmpty();
        RuleFor(x => x.MaintenanceType).IsInEnum();
        RuleFor(x => x.PerformedBy).MaximumLength(100).When(x => x.PerformedBy != null);
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0).When(x => x.Cost != null);
        RuleFor(x => x.NextDueCount).GreaterThan(0).When(x => x.NextDueCount != null);
        RuleFor(x => x.Notes).MaximumLength(300).When(x => x.Notes != null);
    }
}
