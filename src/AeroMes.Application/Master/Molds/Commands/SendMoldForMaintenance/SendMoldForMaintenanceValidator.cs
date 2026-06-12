using FluentValidation;

namespace AeroMes.Application.Master.Molds.Commands.SendMoldForMaintenance;

public class SendMoldForMaintenanceValidator : AbstractValidator<SendMoldForMaintenanceCommand>
{
    public SendMoldForMaintenanceValidator()
    {
        RuleFor(x => x.MoldCode).NotEmpty();
        RuleFor(x => x.MaintenanceType).IsInEnum();
    }
}
