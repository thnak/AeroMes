using FluentValidation;

namespace AeroMes.Application.Master.FabricRolls.Commands.RegisterFabricRoll;

public class RegisterFabricRollValidator : AbstractValidator<RegisterFabricRollCommand>
{
    public RegisterFabricRollValidator()
    {
        RuleFor(x => x.RollBarcode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FabricProductCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LotNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ShadeCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.GrossLengthMeters).GreaterThan(0);
        RuleFor(x => x.GrossWeightKg).GreaterThan(0);
        RuleFor(x => x.FabricWidthCm).GreaterThan(0);
        RuleFor(x => x.SupplierCode).MaximumLength(50).When(x => x.SupplierCode != null);
    }
}
