using AeroMes.Application.Interfaces;
using AeroMes.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Application.WorkOrders.Commands.StartWorkOrder;

public class StartWorkOrderHandler(IApplicationDbContext db)
    : IRequestHandler<StartWorkOrderCommand, StartWorkOrderResult>
{
    public async Task<StartWorkOrderResult> Handle(StartWorkOrderCommand cmd, CancellationToken ct)
    {
        var workOrder = await db.WorkOrders
            .FirstOrDefaultAsync(x => x.WorkOrderID == cmd.WorkOrderId, ct)
            ?? throw new KeyNotFoundException($"WorkOrder {cmd.WorkOrderId} not found.");

        if (workOrder.Status != WorkOrderStatus.Released)
            throw new InvalidOperationException(
                $"WorkOrder {workOrder.WorkOrderNo} cannot be started from status {workOrder.Status}.");

        workOrder.Status = WorkOrderStatus.Running;
        workOrder.ActualStartDate = cmd.Timestamp;
        workOrder.UpdatedBy = cmd.OperatorId;
        workOrder.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return new StartWorkOrderResult(
            workOrder.WorkOrderID,
            workOrder.Status.ToString().ToUpperInvariant(),
            workOrder.ActualStartDate!.Value);
    }
}
