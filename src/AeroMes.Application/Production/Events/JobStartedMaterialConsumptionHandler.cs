using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Events;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Events.Abstractions;

namespace AeroMes.Application.Production.Events;

public class JobStartedMaterialConsumptionHandler(
    IJobRepository jobRepo,
    IWorkOrderRepository workOrderRepo,
    IBomItemRepository bomItemRepo,
    IMaterialConsumptionRepository consumptionRepo)
    : IEventHandler<JobStartedEvent>
{
    public async Task HandleAsync(JobStartedEvent @event, CancellationToken ct)
    {
        var job = await jobRepo.GetByIdAsync(@event.JobID, ct);
        if (job is null) return;

        var workOrder = await workOrderRepo.GetByIdWithRoutingStepAndProductionOrderAsync(job.WOID, ct);
        if (workOrder?.ProductionOrder is null) return;

        var productCode = workOrder.ProductionOrder.ProductCode;
        var bomItems = await bomItemRepo.GetByParentAsync(productCode, ct);
        if (!bomItems.Any()) return;

        var targetQty = workOrder.TargetQuantity.Value;
        var planned = bomItems
            .Where(b => b.IsActive)
            .Select(b => MaterialConsumption.CreatePlanned(
                @event.JobID,
                b.ChildProductCode,
                b.RequiredQty * targetQty * (1m + b.ScrapFactor / 100m)))
            .ToList();

        await consumptionRepo.AddRangeAsync(planned, ct);
        await consumptionRepo.SaveChangesAsync(ct);
    }
}
