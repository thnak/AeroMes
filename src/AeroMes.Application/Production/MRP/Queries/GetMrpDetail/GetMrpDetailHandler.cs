using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.MRP.Queries.GetMrpDetail;

public class GetMrpDetailHandler(IMaterialRequirementsPlanRepository repository)
    : IQueryHandler<GetMrpDetailQuery, MrpDetailDto?>
{
    public Task<MrpDetailDto?> HandleAsync(GetMrpDetailQuery query, CancellationToken ct)
        => repository.GetDetailAsync(query.MrpID, ct);
}
