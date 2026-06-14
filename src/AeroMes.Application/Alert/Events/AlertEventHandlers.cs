using AeroMes.Domain.Production.Events;
using LiteBus.Events.Abstractions;

namespace AeroMes.Application.Alert.Events;

public class OutputSubmittedAlertHandler(AlertEvaluationService alertService)
    : IEventHandler<WorkOrderOutputSubmittedEvent>
{
    public async Task HandleAsync(WorkOrderOutputSubmittedEvent @event, CancellationToken ct)
    {
        var total = @event.QtyOK + @event.QtyNG;
        if (total > 0)
        {
            var rejectionRate = Math.Round((decimal)@event.QtyNG / total * 100, 2);
            await alertService.EvaluateAsync("REJECTION_RATE", null, rejectionRate, ct);
        }
    }
}

public class DowntimeStartedAlertHandler(AlertEvaluationService alertService)
    : IEventHandler<DowntimeStartedEvent>
{
    public Task HandleAsync(DowntimeStartedEvent @event, CancellationToken ct)
        => alertService.EvaluateAsync("DOWNTIME_STARTED", @event.MachineCode, 1m, ct);
}
