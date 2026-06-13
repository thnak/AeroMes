using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Operations.Commands.UpdateOperation;

public class UpdateOperationValidator : AbstractValidator<UpdateOperationCommand>
{
    public UpdateOperationValidator(IOperationRepository repo)
    {
        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Operation code is required.")
            .MustAsync(async (code, ct) => await repo.ExistsAsync(code, ct))
            .WithMessage(x => $"Operation '{x.Code}' does not exist.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must be at most 500 characters.")
            .When(x => x.Description is not null);
    }
}
