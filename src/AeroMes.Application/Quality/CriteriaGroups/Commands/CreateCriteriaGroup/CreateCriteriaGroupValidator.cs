using FluentValidation;

namespace AeroMes.Application.Quality.CriteriaGroups.Commands.CreateCriteriaGroup;

public class CreateCriteriaGroupValidator : AbstractValidator<CreateCriteriaGroupCommand>
{
    public CreateCriteriaGroupValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
