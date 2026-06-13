using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetBlendLogForJob;

public class GetBlendLogForJobHandler(IMaterialBlendLogRepository repo)
    : IQueryHandler<GetBlendLogForJobQuery, IReadOnlyList<MaterialBlendLogDto>>
{
    public Task<IReadOnlyList<MaterialBlendLogDto>> HandleAsync(GetBlendLogForJobQuery query, CancellationToken ct)
        => repo.GetByJobAsync(query.JobID, ct);
}
