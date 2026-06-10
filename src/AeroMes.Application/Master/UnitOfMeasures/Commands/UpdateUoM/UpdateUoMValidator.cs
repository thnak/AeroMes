using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.UnitOfMeasures.Commands.UpdateUoM;

public class UpdateUoMValidator : AbstractValidator<UpdateUoMCommand>
{
    private static readonly string[] ValidGroups = ["QUANTITY", "WEIGHT", "LENGTH", "VOLUME", "AREA", "TIME"];

    public UpdateUoMValidator(IUnitOfMeasureRepository repo)
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MustAsync(async (code, ct) => await repo.CodeExistsAsync(code, ct))
            .WithMessage(x => $"UoM '{x.Code}' does not exist.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Group)
            .NotEmpty()
            .Must(g => ValidGroups.Contains(g.ToUpperInvariant()))
            .WithMessage($"Group must be one of: {string.Join(", ", ValidGroups)}.");
    }
}
