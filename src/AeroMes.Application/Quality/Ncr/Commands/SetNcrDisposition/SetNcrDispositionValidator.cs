using FluentValidation;

namespace AeroMes.Application.Quality.Ncr.Commands.SetNcrDisposition;

public class SetNcrDispositionValidator : AbstractValidator<SetNcrDispositionCommand>
{
    private static readonly string[] ValidDispositions = ["REWORK", "SCRAP", "USE_AS_IS", "RETURN_TO_SUPPLIER", "RE_INSPECT"];

    public SetNcrDispositionValidator()
    {
        RuleFor(x => x.NcrId).GreaterThan(0);
        RuleFor(x => x.DispositionCode).NotEmpty()
            .Must(d => ValidDispositions.Contains(d))
            .WithMessage($"DispositionCode must be one of: {string.Join(", ", ValidDispositions)}.");
        RuleFor(x => x.SetBy).NotEmpty().MaximumLength(100);
    }
}
