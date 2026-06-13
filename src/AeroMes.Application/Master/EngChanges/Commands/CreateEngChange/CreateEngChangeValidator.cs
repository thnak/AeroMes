using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.EngChanges.Commands.CreateEngChange;

public class CreateEngChangeValidator : AbstractValidator<CreateEngChangeCommand>
{
    public CreateEngChangeValidator(IEngChangeRepository repo)
    {
        RuleFor(x => x.EcNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.EcType).IsInEnum();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description != null);
        RuleFor(x => x.Reason).IsInEnum();
        RuleFor(x => x.Priority).IsInEnum();
        RuleFor(x => x.AffectedProducts).MaximumLength(500).When(x => x.AffectedProducts != null);
        RuleFor(x => x.EcNumber)
            .Cascade(CascadeMode.Stop)
            .MustAsync(async (number, ct) => !await repo.NumberExistsAsync(number, ct))
            .WithMessage(x => $"Số phiếu '{x.EcNumber}' đã tồn tại.");
    }
}
