using AeroMes.Domain.Quality.Repositories;
using FluentValidation;

namespace AeroMes.Application.Quality.DefectCodes.Commands.CreateDefectCode;

public class CreateDefectCodeValidator : AbstractValidator<CreateDefectCodeCommand>
{
    public CreateDefectCodeValidator(IDefectCodeRepository repo)
    {
        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(30)
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("Code may only contain letters, digits, hyphens, and underscores.")
            .MustAsync(async (code, ct) => !await repo.CodeExistsAsync(code, ct))
            .WithMessage(x => $"DefectCode '{x.Code}' already exists.");

        RuleFor(x => x.DefectName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DefectCategory).MaximumLength(100).When(x => x.DefectCategory is not null);
    }
}
