using FluentValidation;

namespace AeroMes.Application.Quality.Criteria.Commands.CreateCriteria;

public class CreateCriteriaValidator : AbstractValidator<CreateCriteriaCommand>
{
    public CreateCriteriaValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.InspectionMethod).MaximumLength(200);
        RuleFor(x => x.MethodDescription).MaximumLength(2000);
    }
}
