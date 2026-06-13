using AeroMes.Application.Interfaces;
using AeroMes.Application.Modules.Queries.GetModuleStatus;
using AeroMes.Domain.Production.Events;
using LiteBus.Events.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace AeroMes.Application.Modules.Events;

public class DowntimeStartedModuleHandler(
    IMemoryCache cache, IModuleStatusRepository statusRepo, IModuleStatusNotifier notifier)
    : IEventHandler<DowntimeStartedEvent>
{
    public Task HandleAsync(DowntimeStartedEvent @event, CancellationToken ct) =>
        ProductionModuleInvalidation.InvalidateAndNotifyAsync(cache, statusRepo, notifier, ct);
}

public class DowntimeEndedModuleHandler(
    IMemoryCache cache, IModuleStatusRepository statusRepo, IModuleStatusNotifier notifier)
    : IEventHandler<DowntimeEndedEvent>
{
    public Task HandleAsync(DowntimeEndedEvent @event, CancellationToken ct) =>
        ProductionModuleInvalidation.InvalidateAndNotifyAsync(cache, statusRepo, notifier, ct);
}
