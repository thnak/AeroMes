using FluentValidation;

namespace AeroMes.Application.Quality.Ncr.Commands.CancelNcr;

public class CancelNcrValidator : AbstractValidator<CancelNcrCommand>
{
    public CancelNcrValidator()
    {
        RuleFor(x => x.NcrId).GreaterThan(0);
        RuleFor(x => x.CancelledBy).NotEmpty().MaximumLength(100);
    }
}
