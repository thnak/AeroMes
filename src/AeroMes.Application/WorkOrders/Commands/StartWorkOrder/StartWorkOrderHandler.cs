using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.WorkOrders.Commands.StartWorkOrder;

public class StartWorkOrderHandler(
    IWorkOrderRepository workOrderRepo,
    IUnitOfWork uow,
    IValidator<StartWorkOrderCommand> validator)
    : ICommandHandler<StartWorkOrderCommand, ValidationResult<StartWorkOrderResult>>
{
    public async Task<ValidationResult<StartWorkOrderResult>> HandleAsync(StartWorkOrderCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<StartWorkOrderResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            var workOrder = await workOrderRepo.GetByIdAsync(cmd.WorkOrderId, ct);
            if (workOrder is null) return ValidationResult<StartWorkOrderResult>.NotFound($"Entity '{cmd.WorkOrderId}' was not found.");

            workOrder.Start(cmd.OperatorId, cmd.Timestamp);
            await uow.SaveChangesAsync(ct);

            var result = new StartWorkOrderResult(
                workOrder.WOID,
                workOrder.Status.ToString().ToUpperInvariant(),
                workOrder.ActualStartDate!.Value);
            return ValidationResult<StartWorkOrderResult>.Ok(result);
        }        catch (DomainException ex)
        {
            return ValidationResult<StartWorkOrderResult>.Failure(ex.Message);
        }
    }
}
