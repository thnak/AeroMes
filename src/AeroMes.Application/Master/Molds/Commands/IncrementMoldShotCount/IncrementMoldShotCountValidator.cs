using FluentValidation;

namespace AeroMes.Application.Master.Molds.Commands.IncrementMoldShotCount;

public class IncrementMoldShotCountValidator : AbstractValidator<IncrementMoldShotCountCommand>
{
    public IncrementMoldShotCountValidator()
    {
        RuleFor(x => x.MoldCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.JobID).GreaterThan(0);
        RuleFor(x => x.QtyOK).GreaterThanOrEqualTo(0);
    }
}
