using FluentValidation;

namespace AeroMes.Application.Cost.MachineEnergyProfiles.Commands.UpsertMachineEnergyProfile;

public class UpsertMachineEnergyProfileValidator : AbstractValidator<UpsertMachineEnergyProfileCommand>
{
    public UpsertMachineEnergyProfileValidator()
    {
        RuleFor(x => x.MachineCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.NominalKW).GreaterThan(0);
        RuleFor(x => x.LoadFactor).InclusiveBetween(0.01m, 1.0m)
            .WithMessage("Hệ số tải phải trong khoảng 0.01 đến 1.0.");
    }
}
