using FluentValidation;

namespace AeroMes.Application.Production.Commands.ReworkBundle;

public class ReworkBundleValidator : AbstractValidator<ReworkBundleCommand>
{
    public ReworkBundleValidator()
    {
        RuleFor(x => x.BundleID).GreaterThan(0);
        RuleFor(x => x.TargetOperationCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(300);
    }
}
