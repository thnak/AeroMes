using FluentValidation;

namespace AeroMes.Application.Quality.Ncr.Commands.CloseNcr;

public class CloseNcrValidator : AbstractValidator<CloseNcrCommand>
{
    public CloseNcrValidator()
    {
        RuleFor(x => x.NcrId).GreaterThan(0);
        RuleFor(x => x.ClosedBy).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RootCause).MaximumLength(500).When(x => x.RootCause is not null);
        RuleFor(x => x.CorrectiveAction).MaximumLength(500).When(x => x.CorrectiveAction is not null);
        RuleFor(x => x.PreventiveAction).MaximumLength(500).When(x => x.PreventiveAction is not null);
    }
}
