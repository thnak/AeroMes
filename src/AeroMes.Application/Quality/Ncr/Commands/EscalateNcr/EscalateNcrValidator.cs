using FluentValidation;

namespace AeroMes.Application.Quality.Ncr.Commands.EscalateNcr;

public class EscalateNcrValidator : AbstractValidator<EscalateNcrCommand>
{
    public EscalateNcrValidator()
    {
        RuleFor(x => x.NcrId).GreaterThan(0);
    }
}
