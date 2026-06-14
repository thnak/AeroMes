using AeroMes.Domain.Production.Events;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Events.Abstractions;

namespace AeroMes.Application.Traceability.Events;

public class WorkOrderOutputLotLineageHandler(
    IWorkOrderRepository workOrderRepo,
    IMaterialConsumptionRepository consumptionRepo,
    ILotTraceabilityRepository traceRepo)
    : IEventHandler<WorkOrderOutputSubmittedEvent>
{
    public async Task HandleAsync(WorkOrderOutputSubmittedEvent @event, CancellationToken ct)
    {
        var workOrder = await workOrderRepo.GetByIdWithRoutingStepAndProductionOrderAsync(@event.WOID, ct);
        if (workOrder is null) return;

        var outputLot = @event.WOCode;
        var outputProduct = workOrder.ProductionOrder?.ProductCode ?? string.Empty;
        var now = DateTime.UtcNow;

        var consumptions = await consumptionRepo.GetByWorkOrderAsync(@event.WOID, ct);
        var confirmedInputs = consumptions
            .Where(c => c.LotNumber is not null && c.ActualQty > 0)
            .GroupBy(c => c.LotNumber!)
            .Select(g => (LotNumber: g.Key, ProductCode: g.First().ProductCode, Qty: g.Sum(x => x.ActualQty)))
            .ToList();

        foreach (var input in confirmedInputs)
        {
            var edge = LotLineage.Record(
                input.LotNumber, outputLot, LineageType.Transform,
                workOrderId: @event.WOID, routingStepId: null,
                quantityConsumed: input.Qty, uom: null);
            await traceRepo.AddLineageAsync(edge, ct);
        }

        var producedEvent = LotEvent.Append(
            LotEventType.Produced, outputLot, outputProduct,
            @event.OperatorId, now,
            workOrderId: @event.WOID,
            quantity: @event.QtyOK);

        await traceRepo.AddEventAsync(producedEvent, ct);
        await traceRepo.SaveChangesAsync(ct);
    }
}
