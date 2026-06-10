using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Routings.Queries.GetRoutingWithSteps;

public class GetRoutingWithStepsHandler(IRoutingRepository repo)
    : IQueryHandler<GetRoutingWithStepsQuery, RoutingDetailDto?>
{
    public async Task<RoutingDetailDto?> HandleAsync(GetRoutingWithStepsQuery q, CancellationToken ct)
    {
        var routing = await repo.GetByIdWithStepsAsync(q.RoutingId, ct);
        if (routing is null) return null;

        return new RoutingDetailDto(
            routing.RoutingID,
            routing.RoutingCode,
            routing.RoutingName,
            routing.ProductCode,
            routing.IsDefault,
            routing.IsActive,
            routing.Steps.OrderBy(s => s.StepNumber).Select(s => new RoutingStepDto(
                s.RoutingStepID,
                s.StepNumber,
                s.OperationCode,
                s.DefaultWorkCenterID,
                s.StandardCycleTime,
                s.IsQcRequired)).ToList());
    }
}
