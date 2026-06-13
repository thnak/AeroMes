using AeroMes.Application.Interfaces;
using AeroMes.Domain.Production.Events;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Events.Abstractions;
using Microsoft.Extensions.Logging;

namespace AeroMes.Application.Quality.InspectionOrders.Events;

public class CreateInspectionOrderOnJobFinished(
    IJobRepository jobRepo,
    IWorkOrderRepository workOrderRepo,
    IInspectionPlanRepository planRepo,
    IInspectionOrderRepository orderRepo,
    IUnitOfWork uow,
    ILogger<CreateInspectionOrderOnJobFinished> logger)
    : IEventHandler<JobFinishedEvent>
{
    public async Task HandleAsync(JobFinishedEvent @event, CancellationToken ct)
    {
        var job = await jobRepo.GetByIdAsync(@event.JobID, ct);
        if (job is null)
        {
            logger.LogWarning("CreateInspectionOrderOnJobFinished: Job {JobId} not found.", @event.JobID);
            return;
        }

        var workOrder = await workOrderRepo.GetByIdWithRoutingStepAndProductionOrderAsync(@event.WOID, ct);
        if (workOrder is null)
        {
            logger.LogWarning("CreateInspectionOrderOnJobFinished: WorkOrder {WOID} not found.", @event.WOID);
            return;
        }

        var step = workOrder.RoutingStep;
        if (step is null || !step.IsQcRequired)
            return;

        // Resolve inspection plan: prefer product-specific, fall back to step-wide
        var plans = await planRepo.GetListAsync(step.RoutingStepID, null, true, ct);
        if (plans.Count == 0)
        {
            logger.LogWarning(
                "CreateInspectionOrderOnJobFinished: No active InspectionPlan for RoutingStep {StepId}. Skipping.",
                step.RoutingStepID);
            return;
        }

        // Get the ProductCode from the work order's production order
        var productCode = workOrder.ProductionOrder?.ProductCode ?? string.Empty;

        var plan = plans.FirstOrDefault(p => p.ProductCode == productCode)
                   ?? plans.FirstOrDefault(p => p.ProductCode is null)
                   ?? plans[0];

        var sampleSize = plan.SamplingMethod switch
        {
            "FULL" => (int)workOrder.TargetQuantity.Value,
            "FIXED_N" => plan.SampleSize ?? 1,
            "AQL" => plan.SampleSize ?? 5,
            _ => plan.SampleSize ?? 1,
        };

        var orderNo = $"QC-{DateTime.UtcNow:yyyy}-{@event.JobID:D5}";

        var order = InspectionOrder.Create(
            orderNo,
            plan.PlanId,
            @event.JobID,
            (long)@event.WOID,
            productCode,
            null,
            sampleSize,
            "AUTO_ON_STEP_COMPLETE");

        orderRepo.Add(order);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation(
            "CreateInspectionOrderOnJobFinished: Created InspectionOrder {OrderNo} for Job {JobId}.",
            orderNo, @event.JobID);
    }
}
