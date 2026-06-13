using AeroMes.Application.Wms.Services;
using AeroMes.Domain.Wms.Events;
using LiteBus.Events.Abstractions;

namespace AeroMes.Application.Wms.Events;

public class StockPolicyEvaluationHandler(IStockPolicyEvaluationService evaluationService)
    : IEventHandler<StockMovementCreatedEvent>
{
    public Task HandleAsync(StockMovementCreatedEvent @event, CancellationToken ct) =>
        evaluationService.EvaluateAsync(@event.ProductCode, @event.LocationId, ct);
}
