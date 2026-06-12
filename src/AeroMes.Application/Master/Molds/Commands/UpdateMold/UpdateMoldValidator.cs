using FluentValidation;

namespace AeroMes.Application.Master.Molds.Commands.UpdateMold;

public class UpdateMoldValidator : AbstractValidator<UpdateMoldCommand>
{
    public UpdateMoldValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.MoldType).IsInEnum();
        RuleFor(x => x.Material).MaximumLength(100).When(x => x.Material != null);
        RuleFor(x => x.Cavities).GreaterThanOrEqualTo(1);
        RuleFor(x => x.MaxShots).GreaterThan(0);
        RuleFor(x => x.PmIntervalShots).GreaterThan(0);
        RuleFor(x => x.Manufacturer).MaximumLength(150).When(x => x.Manufacturer != null);
        RuleFor(x => x.PurchaseCost).GreaterThanOrEqualTo(0).When(x => x.PurchaseCost != null);
        RuleFor(x => x.WeightKg).GreaterThan(0).When(x => x.WeightKg != null);
        RuleFor(x => x.StorageLocation).MaximumLength(100).When(x => x.StorageLocation != null);
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes != null);
    }
}
