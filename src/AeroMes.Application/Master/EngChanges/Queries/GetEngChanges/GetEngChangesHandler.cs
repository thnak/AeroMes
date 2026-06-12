using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.EngChanges.Queries.GetEngChanges;

public class GetEngChangesHandler(IEngChangeRepository repo)
    : IQueryHandler<GetEngChangesQuery, IReadOnlyList<EngChangeDto>>
{
    public async Task<IReadOnlyList<EngChangeDto>> HandleAsync(
        GetEngChangesQuery query, CancellationToken ct)
    {
        var changes = await repo.GetAllAsync(query.Status, query.EcType, query.Search, ct);
        return changes.Select(ToDto).ToList();
    }

    internal static EngChangeDto ToDto(EngChange ec) => new(
        ec.EcId, ec.EcNumber, ec.EcType.ToString(), ec.Title,
        ec.Reason.ToString(), ec.Status.ToString(), ec.Priority.ToString(),
        ec.RequestedBy, ec.RequestedAt, ec.TargetDate,
        ec.ApprovedBy, ec.ApprovedAt, ec.AffectedProducts,
        ec.SourceEcrNumber, ec.NewBomHeaderId);
}
