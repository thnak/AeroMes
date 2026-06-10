using FluentValidation;

namespace AeroMes.Application.Master.MachineProductConfigs.Commands.UpsertMachineProductConfig;

public class UpsertMachineProductConfigValidator : AbstractValidator<UpsertMachineProductConfigCommand>
{
    public UpsertMachineProductConfigValidator()
    {
        RuleFor(x => x.MachineCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.IdealCycleTimeSeconds).GreaterThan(0);
        RuleFor(x => x.TargetThroughputPerHour).GreaterThan(0);
        RuleFor(x => x.SetupTimeSeconds).GreaterThanOrEqualTo(0);
    }
}
