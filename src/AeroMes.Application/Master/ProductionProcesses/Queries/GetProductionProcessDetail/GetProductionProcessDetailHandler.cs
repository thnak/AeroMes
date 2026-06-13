using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductionProcesses.Queries.GetProductionProcessDetail;

public class GetProductionProcessDetailHandler(IProductionProcessRepository repository)
    : IQueryHandler<GetProductionProcessDetailQuery, ProductionProcessDetailDto?>
{
    public Task<ProductionProcessDetailDto?> HandleAsync(
        GetProductionProcessDetailQuery query, CancellationToken ct)
        => repository.GetDetailAsync(query.ProcessID, ct);
}
