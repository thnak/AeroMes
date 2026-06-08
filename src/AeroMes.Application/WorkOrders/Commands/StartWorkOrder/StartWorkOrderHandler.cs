using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using MediatR;

namespace AeroMes.Application.WorkOrders.Commands.StartWorkOrder;

public class StartWorkOrderHandler(
    IWorkOrderRepository workOrderRepo,
    IUnitOfWork uow)
    : IRequestHandler<StartWorkOrderCommand, StartWorkOrderResult>
{
    public async Task<StartWorkOrderResult> Handle(StartWorkOrderCommand cmd, CancellationToken ct)
    {
        var workOrder = await workOrderRepo.GetByIdAsync(cmd.WorkOrderId, ct)
            ?? throw new EntityNotFoundException(nameof(WorkOrder), cmd.WorkOrderId);

        workOrder.Start(cmd.OperatorId, cmd.Timestamp);
        await uow.SaveChangesAsync(ct);

        return new StartWorkOrderResult(
            workOrder.WOID,
            workOrder.Status.ToString().ToUpperInvariant(),
            workOrder.ActualStartDate!.Value);
    }
}
