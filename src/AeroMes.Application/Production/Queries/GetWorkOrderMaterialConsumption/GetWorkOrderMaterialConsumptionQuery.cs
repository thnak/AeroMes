using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetWorkOrderMaterialConsumption;

public record GetWorkOrderMaterialConsumptionQuery(int WorkOrderId)
    : IQuery<IReadOnlyList<MaterialConsumptionDto>>;

public class GetWorkOrderMaterialConsumptionHandler(IMaterialConsumptionRepository repo)
    : IQueryHandler<GetWorkOrderMaterialConsumptionQuery, IReadOnlyList<MaterialConsumptionDto>>
{
    public Task<IReadOnlyList<MaterialConsumptionDto>> HandleAsync(
        GetWorkOrderMaterialConsumptionQuery query, CancellationToken ct)
        => repo.GetByWorkOrderAsync(query.WorkOrderId, ct);
}
