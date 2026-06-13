using AeroMes.Application.Interfaces;
using AeroMes.Application.Modules.Queries.GetModuleStatus;
using AeroMes.Domain.Production.Events;
using LiteBus.Events.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace AeroMes.Application.Modules.Events;


public class WorkOrderStartedModuleHandler(
    IMemoryCache cache, IModuleStatusRepository statusRepo, IModuleStatusNotifier notifier)
    : IEventHandler<WorkOrderStartedEvent>
{
    public Task HandleAsync(WorkOrderStartedEvent @event, CancellationToken ct) =>
        ProductionModuleInvalidation.InvalidateAndNotifyAsync(cache, statusRepo, notifier, ct);
}

public class WorkOrderCompletedModuleHandler(
    IMemoryCache cache, IModuleStatusRepository statusRepo, IModuleStatusNotifier notifier)
    : IEventHandler<WorkOrderCompletedEvent>
{
    public Task HandleAsync(WorkOrderCompletedEvent @event, CancellationToken ct) =>
        ProductionModuleInvalidation.InvalidateAndNotifyAsync(cache, statusRepo, notifier, ct);
}

public class WorkOrderPausedModuleHandler(
    IMemoryCache cache, IModuleStatusRepository statusRepo, IModuleStatusNotifier notifier)
    : IEventHandler<WorkOrderPausedEvent>
{
    public Task HandleAsync(WorkOrderPausedEvent @event, CancellationToken ct) =>
        ProductionModuleInvalidation.InvalidateAndNotifyAsync(cache, statusRepo, notifier, ct);
}

public class WorkOrderResumedModuleHandler(
    IMemoryCache cache, IModuleStatusRepository statusRepo, IModuleStatusNotifier notifier)
    : IEventHandler<WorkOrderResumedEvent>
{
    public Task HandleAsync(WorkOrderResumedEvent @event, CancellationToken ct) =>
        ProductionModuleInvalidation.InvalidateAndNotifyAsync(cache, statusRepo, notifier, ct);
}
