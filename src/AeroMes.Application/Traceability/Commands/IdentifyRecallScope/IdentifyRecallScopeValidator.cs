using FluentValidation;

namespace AeroMes.Application.Traceability.Commands.IdentifyRecallScope;

public sealed class IdentifyRecallScopeValidator : AbstractValidator<IdentifyRecallScopeCommand>
{
    public IdentifyRecallScopeValidator()
    {
        RuleFor(x => x.RecallID).NotEmpty();
        RuleFor(x => x.RequestedBy).NotEmpty().MaximumLength(50);
    }
}
