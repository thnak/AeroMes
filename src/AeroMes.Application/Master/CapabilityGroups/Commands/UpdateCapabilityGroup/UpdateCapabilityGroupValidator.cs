using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.CapabilityGroups.Commands.UpdateCapabilityGroup;

public class UpdateCapabilityGroupValidator : AbstractValidator<UpdateCapabilityGroupCommand>
{
    public UpdateCapabilityGroupValidator(ICapabilityGroupRepository repo)
    {
        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Capability group code is required.")
            .MustAsync(async (code, ct) => await repo.ExistsAsync(code, ct))
            .WithMessage(x => $"Capability group '{x.Code}' does not exist.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must be at most 500 characters.")
            .When(x => x.Description is not null);
    }
}
