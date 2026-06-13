using FluentValidation;

namespace AeroMes.Application.Master.Machines.Commands.DuplicateMachine;

public class DuplicateMachineValidator : AbstractValidator<DuplicateMachineCommand>
{
    public DuplicateMachineValidator()
    {
        RuleFor(x => x.SourceCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.NewCode).NotEmpty().MaximumLength(50);
    }
}
