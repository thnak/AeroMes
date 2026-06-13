using FluentValidation;

namespace AeroMes.Application.Wms.Commands.SealCarton;

public class SealCartonValidator : AbstractValidator<SealCartonCommand>
{
    public SealCartonValidator()
    {
        RuleFor(x => x.CartonId).GreaterThan(0);
        RuleFor(x => x.GrossWeightKg).GreaterThan(0).When(x => x.GrossWeightKg.HasValue);
    }
}
