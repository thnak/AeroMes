using FluentValidation;

namespace AeroMes.Application.Production.Commands.ReceiveBundleAtStation;

public class ReceiveBundleAtStationValidator : AbstractValidator<ReceiveBundleAtStationCommand>
{
    public ReceiveBundleAtStationValidator()
    {
        RuleFor(x => x.BundleBarcode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.OperationCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.WorkCenterID).GreaterThan(0);
        RuleFor(x => x.OperatorID).NotEmpty().MaximumLength(50);
    }
}
