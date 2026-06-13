using FluentValidation;

namespace AeroMes.Application.Traceability.Commands.InitiateRecall;

public sealed class InitiateRecallValidator : AbstractValidator<InitiateRecallCommand>
{
    public InitiateRecallValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AnchorLotNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.InitiatedBy).NotEmpty().MaximumLength(50);
        RuleFor(x => x.RegulatoryRef).MaximumLength(100).When(x => x.RegulatoryRef is not null);
    }
}
