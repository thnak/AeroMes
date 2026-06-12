using AeroMes.Domain.Quality.Repositories;
using FluentValidation;

namespace AeroMes.Application.Quality.DefectCodes.Commands.UpdateDefectCode;

public class UpdateDefectCodeValidator : AbstractValidator<UpdateDefectCodeCommand>
{
    public UpdateDefectCodeValidator(IDefectCodeRepository repo)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .MustAsync(async (id, ct) => await repo.GetByIdAsync(id, ct) is not null)
            .WithMessage(x => $"DefectCode {x.Id} does not exist.");

        RuleFor(x => x.DefectName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DefectCategory).MaximumLength(100).When(x => x.DefectCategory is not null);
    }
}
