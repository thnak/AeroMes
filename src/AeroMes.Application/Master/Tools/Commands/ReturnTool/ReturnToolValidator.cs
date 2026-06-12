using FluentValidation;

namespace AeroMes.Application.Master.Tools.Commands.ReturnTool;

public class ReturnToolValidator : AbstractValidator<ReturnToolCommand>
{
    public ReturnToolValidator()
    {
        RuleFor(x => x.ToolCode).NotEmpty();
        RuleFor(x => x.ReturnedBy).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Condition).IsInEnum();
        RuleFor(x => x.Notes).MaximumLength(255).When(x => x.Notes != null);
    }
}
