using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.EngChanges.Commands.CreateEcoFromEcr;

public class CreateEcoFromEcrValidator : AbstractValidator<CreateEcoFromEcrCommand>
{
    public CreateEcoFromEcrValidator(IEngChangeRepository repo)
    {
        RuleFor(x => x.EcrNumber).NotEmpty();
        RuleFor(x => x.NewEcNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.NewEcNumber)
            .Cascade(CascadeMode.Stop)
            .MustAsync(async (number, ct) => !await repo.NumberExistsAsync(number, ct))
            .WithMessage(x => $"Số phiếu '{x.NewEcNumber}' đã tồn tại.");
    }
}
