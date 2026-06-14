using AeroMes.Application.Interfaces;
using AeroMes.Domain.Production.Events;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Events.Abstractions;

namespace AeroMes.Application.ShopFloor.Events;

public class WorkOrderStartedShopFloorHandler(
    IWorkOrderRepository workOrderRepo,
    IShopFloorNotifier notifier)
    : IEventHandler<WorkOrderStartedEvent>
{
    public async Task HandleAsync(WorkOrderStartedEvent @event, CancellationToken ct)
    {
        var wo = await workOrderRepo.GetByIdAsync(@event.WOID, ct);
        var workCenterId = wo?.WorkCenterID ?? 0;
        await notifier.WorkOrderStartedAsync(@event.WOID, @event.WOCode, workCenterId, ct);
    }
}

public class WorkOrderCompletedShopFloorHandler(IShopFloorNotifier notifier)
    : IEventHandler<WorkOrderCompletedEvent>
{
    public Task HandleAsync(WorkOrderCompletedEvent @event, CancellationToken ct) =>
        notifier.WorkOrderCompletedAsync(@event.WOID, @event.WOCode, ct);
}

public class WorkOrderOutputSubmittedShopFloorHandler(
    IWorkOrderRepository workOrderRepo,
    IShopFloorNotifier notifier)
    : IEventHandler<WorkOrderOutputSubmittedEvent>
{
    public async Task HandleAsync(WorkOrderOutputSubmittedEvent @event, CancellationToken ct)
    {
        var wo = await workOrderRepo.GetByIdAsync(@event.WOID, ct);
        var target = wo?.TargetQuantity.Value ?? 1;
        var completionPct = target > 0 ? Math.Round((double)@event.QtyOK / target * 100, 1) : 0;
        await notifier.WorkOrderProgressUpdatedAsync(@event.WOID, @event.WOCode, @event.QtyOK, @event.QtyNG, completionPct, ct);
    }
}

public class DowntimeStartedShopFloorHandler(IShopFloorNotifier notifier)
    : IEventHandler<DowntimeStartedEvent>
{
    public Task HandleAsync(DowntimeStartedEvent @event, CancellationToken ct) =>
        notifier.MachineStatusChangedAsync(@event.MachineCode, "DOWN", DateTimeOffset.UtcNow, ct);
}

public class DowntimeEndedShopFloorHandler(IShopFloorNotifier notifier)
    : IEventHandler<DowntimeEndedEvent>
{
    public Task HandleAsync(DowntimeEndedEvent @event, CancellationToken ct) =>
        notifier.MachineStatusChangedAsync(@event.MachineCode, "IDLE", DateTimeOffset.UtcNow, ct);
}
