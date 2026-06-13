using FluentValidation;

namespace AeroMes.Application.Traceability.Commands.PlaceHold;

public sealed class PlaceHoldValidator : AbstractValidator<PlaceHoldCommand>
{
    public PlaceHoldValidator()
    {
        RuleFor(x => x.LotNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.InitiatedBy).NotEmpty().MaximumLength(50);
        RuleFor(x => x.HoldDescription).MaximumLength(500).When(x => x.HoldDescription is not null);
        RuleFor(x => x.HoldReference).MaximumLength(100).When(x => x.HoldReference is not null);
    }
}
