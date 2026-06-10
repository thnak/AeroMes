using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Products.Commands.ChangeLifecycleStatus;

public class ChangeLifecycleStatusValidator : AbstractValidator<ChangeLifecycleStatusCommand>
{
    public ChangeLifecycleStatusValidator(IProductRepository repo)
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MustAsync(async (code, ct) => await repo.ExistsAsync(code, ct))
            .WithMessage(x => $"Product '{x.Code}' does not exist.");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty();
    }
}
