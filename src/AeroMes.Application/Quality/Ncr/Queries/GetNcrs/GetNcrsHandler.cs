using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.Ncr.Queries.GetNcrs;

public class GetNcrsHandler(INcrRepository repo) : IQueryHandler<GetNcrsQuery, IReadOnlyList<NcrListDto>>
{
    public async Task<IReadOnlyList<NcrListDto>> HandleAsync(GetNcrsQuery q, CancellationToken ct)
    {
        var ncrs = await repo.GetListAsync(q.Status, q.ProductCode, ct);
        return ncrs.Select(n => new NcrListDto(
            n.NcrId,
            n.NcrNo,
            n.Status,
            n.Severity,
            n.ProductCode,
            n.QtyAffected,
            n.DispositionCode,
            n.AssignedTo,
            n.DueDate,
            n.CreatedAt,
            n.InspectionOrderId)).ToList();
    }
}
