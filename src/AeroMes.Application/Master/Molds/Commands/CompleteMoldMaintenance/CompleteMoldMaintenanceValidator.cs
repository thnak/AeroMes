using FluentValidation;

namespace AeroMes.Application.Master.Molds.Commands.CompleteMoldMaintenance;

public class CompleteMoldMaintenanceValidator : AbstractValidator<CompleteMoldMaintenanceCommand>
{
    public CompleteMoldMaintenanceValidator()
    {
        RuleFor(x => x.MoldCode).NotEmpty();
        RuleFor(x => x.MaintenanceType).IsInEnum();
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate != null)
            .WithMessage("Ngày kết thúc bảo trì phải sau ngày bắt đầu.");
        RuleFor(x => x.TechnicianId).MaximumLength(100).When(x => x.TechnicianId != null);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description != null);
        RuleFor(x => x.PartReplaced).MaximumLength(300).When(x => x.PartReplaced != null);
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0).When(x => x.Cost != null);
        RuleFor(x => x.NextDueShots).GreaterThan(0).When(x => x.NextDueShots != null);
    }
}
