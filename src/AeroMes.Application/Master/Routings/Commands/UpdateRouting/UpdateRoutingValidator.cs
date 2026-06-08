using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Routings.Commands.UpdateRouting;

public class UpdateRoutingValidator : AbstractValidator<UpdateRoutingCommand>
{
    public UpdateRoutingValidator(IRoutingRepository repo)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Routing id is required.")
            .MustAsync(async (id, ct) => await repo.ExistsAsync(id, ct))
            .WithMessage(x => $"Routing {x.Id} does not exist.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty().WithMessage("UpdatedBy is required.");
    }
}
