using FluentValidation;

namespace AeroMes.Application.Quality.SamplingMethods.Commands.CreateSamplingMethod;

public class CreateSamplingMethodValidator : AbstractValidator<CreateSamplingMethodCommand>
{
    public CreateSamplingMethodValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.MaxDefects).GreaterThanOrEqualTo(0);
    }
}
