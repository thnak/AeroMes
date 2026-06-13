using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetSSCCContents;

public class GetSSCCContentsHandler(ISerialUnitRepository repo)
    : IQueryHandler<GetSSCCContentsQuery, SSCCContentsDto>
{
    public Task<SSCCContentsDto> HandleAsync(GetSSCCContentsQuery query, CancellationToken ct)
        => repo.GetSSCCContentsAsync(query.SSCC, ct);
}
