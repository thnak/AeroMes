using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetRmaList;

public class GetRmaListHandler(IRmaRepository rmaRepo)
    : IQueryHandler<GetRmaListQuery, IReadOnlyList<RmaSummaryDto>>
{
    public async Task<IReadOnlyList<RmaSummaryDto>> HandleAsync(
        GetRmaListQuery query, CancellationToken ct)
    {
        var rmas = await rmaRepo.GetAllAsync(query.Direction, query.Status, ct);

        return rmas.Select(r => new RmaSummaryDto(
            r.RmaId,
            r.RmaCode,
            r.ReturnDirection,
            r.SourceDocumentType,
            r.SourceDocumentId,
            r.ReturnReason,
            r.Status,
            r.AuthorizedBy,
            r.AuthorizedAt,
            r.Lines.Count,
            r.CreatedBy,
            r.CreatedAt)).ToList();
    }
}
