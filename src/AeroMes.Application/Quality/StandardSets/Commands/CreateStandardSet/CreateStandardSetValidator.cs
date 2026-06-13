using FluentValidation;

namespace AeroMes.Application.Quality.StandardSets.Commands.CreateStandardSet;

public class CreateStandardSetValidator : AbstractValidator<CreateStandardSetCommand>
{
    public CreateStandardSetValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ProductCode).NotEmpty();
        RuleFor(x => x.SamplingMethodID).GreaterThan(0);
        RuleFor(x => x.EffectiveDate).NotEmpty();
    }
}
