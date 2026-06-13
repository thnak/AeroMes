using FluentValidation;

namespace AeroMes.Application.Cost.MachineCostRates.Commands.UpsertMachineCostRate;

public class UpsertMachineCostRateValidator : AbstractValidator<UpsertMachineCostRateCommand>
{
    public UpsertMachineCostRateValidator()
    {
        RuleFor(x => x.MachineCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.RatePerHour).GreaterThan(0);
    }
}
