using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.WorkCenters.Commands.UpdateWorkCenter;

public class UpdateWorkCenterValidator : AbstractValidator<UpdateWorkCenterCommand>
{
    public UpdateWorkCenterValidator(IWorkCenterRepository repo)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("WorkCenter id is required.")
            .MustAsync(async (id, ct) => await repo.ExistsAsync(id, ct))
            .WithMessage(x => $"WorkCenter {x.Id} does not exist.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must be at most 500 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.UpdatedBy)
            .NotEmpty().WithMessage("UpdatedBy is required.");
    }
}
