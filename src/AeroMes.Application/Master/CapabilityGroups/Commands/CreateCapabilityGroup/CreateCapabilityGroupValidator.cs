using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.CapabilityGroups.Commands.CreateCapabilityGroup;

public class CreateCapabilityGroupValidator : AbstractValidator<CreateCapabilityGroupCommand>
{
    public CreateCapabilityGroupValidator(ICapabilityGroupRepository repo)
    {
        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(30).WithMessage("Code must be at most 30 characters.")
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("Code may only contain letters, digits, hyphens, and underscores.")
            .MustAsync(async (code, ct) => !await repo.ExistsAsync(code, ct))
            .WithMessage(x => $"Capability group code '{x.Code}' already exists.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must be at most 500 characters.")
            .When(x => x.Description is not null);
    }
}
