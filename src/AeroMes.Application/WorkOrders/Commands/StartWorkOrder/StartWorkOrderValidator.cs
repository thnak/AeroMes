using AeroMes.Domain.Production.Repositories;
using FluentValidation;

namespace AeroMes.Application.WorkOrders.Commands.StartWorkOrder;

public class StartWorkOrderValidator : AbstractValidator<StartWorkOrderCommand>
{
    public StartWorkOrderValidator(IWorkOrderRepository repo)
    {
        RuleFor(x => x.WorkOrderId)
            .GreaterThan(0).WithMessage("WorkOrder id is required.")
            .MustAsync(async (id, ct) => await repo.GetByIdAsync(id, ct) is not null)
            .WithMessage(x => $"WorkOrder {x.WorkOrderId} does not exist.");

        RuleFor(x => x.OperatorId)
            .NotEmpty().WithMessage("OperatorId is required.");
    }
}
