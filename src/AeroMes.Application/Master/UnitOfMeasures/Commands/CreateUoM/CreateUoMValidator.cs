using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.UnitOfMeasures.Commands.CreateUoM;

public class CreateUoMValidator : AbstractValidator<CreateUoMCommand>
{
    private static readonly string[] ValidGroups = ["QUANTITY", "WEIGHT", "LENGTH", "VOLUME", "AREA", "TIME"];

    public CreateUoMValidator(IUnitOfMeasureRepository repo)
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(20)
            .MustAsync(async (code, ct) => !await repo.CodeExistsAsync(code, ct))
            .WithMessage(x => $"UoM code '{x.Code}' already exists.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Group)
            .NotEmpty()
            .Must(g => ValidGroups.Contains(g.ToUpperInvariant()))
            .WithMessage($"Group must be one of: {string.Join(", ", ValidGroups)}.");
    }
}
