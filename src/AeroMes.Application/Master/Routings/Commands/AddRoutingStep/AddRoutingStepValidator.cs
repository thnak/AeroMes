using AeroMes.Domain.Master.Repositories;
using FluentValidation;

namespace AeroMes.Application.Master.Routings.Commands.AddRoutingStep;

public class AddRoutingStepValidator : AbstractValidator<AddRoutingStepCommand>
{
    public AddRoutingStepValidator(IRoutingRepository routingRepo, IOperationRepository opRepo, IWorkCenterRepository wcRepo)
    {
        RuleFor(x => x.RoutingId)
            .GreaterThan(0).WithMessage("Routing id is required.")
            .MustAsync(async (id, ct) => await routingRepo.ExistsAsync(id, ct))
            .WithMessage(x => $"Routing {x.RoutingId} does not exist.");

        RuleFor(x => x.StepNumber)
            .GreaterThan(0).WithMessage("Step number must be greater than zero.");

        RuleFor(x => x.OperationCode)
            .NotEmpty().WithMessage("Operation code is required.")
            .MustAsync(async (code, ct) => await opRepo.ExistsAsync(code, ct))
            .WithMessage(x => $"Operation '{x.OperationCode}' does not exist.");

        RuleFor(x => x.DefaultWorkCenterId)
            .GreaterThan(0).WithMessage("Default work center is required.")
            .MustAsync(async (id, ct) => await wcRepo.ExistsAsync(id, ct))
            .WithMessage(x => $"WorkCenter {x.DefaultWorkCenterId} does not exist.");

        RuleFor(x => x.StandardCycleTime)
            .GreaterThanOrEqualTo(0).WithMessage("Standard cycle time cannot be negative.");
    }
}
