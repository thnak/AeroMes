using FluentValidation;

namespace AeroMes.Application.Maintenance.Commands.CreateMaintenanceOrder;

public class CreateMaintenanceOrderValidator : AbstractValidator<CreateMaintenanceOrderCommand>
{
    public CreateMaintenanceOrderValidator()
    {
        RuleFor(x => x.MaintOrderCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.MachineCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CreatedBy).NotEmpty();
    }
}
