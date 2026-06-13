using FluentValidation;

namespace AeroMes.Application.Quality.Ncr.Commands.UpdateNcr;

public class UpdateNcrValidator : AbstractValidator<UpdateNcrCommand>
{
    public UpdateNcrValidator()
    {
        RuleFor(x => x.NcrId).GreaterThan(0);
        RuleFor(x => x.AssignedTo).MaximumLength(100).When(x => x.AssignedTo is not null);
    }
}
