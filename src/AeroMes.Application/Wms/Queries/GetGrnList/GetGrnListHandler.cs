using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetGrnList;

public class GetGrnListHandler(IGoodsReceiptNoteRepository repo)
    : IQueryHandler<GetGrnListQuery, IReadOnlyList<GrnListDto>>
{
    public async Task<IReadOnlyList<GrnListDto>> HandleAsync(GetGrnListQuery q, CancellationToken ct)
    {
        GrnStatus? status = null;
        if (q.Status.HasValue)
            status = q.Status;

        var grns = await repo.GetAllAsync(status, q.PoId, ct);
        return [.. grns.Select(grn => new GrnListDto(
            grn.GrnId, grn.GrnCode, grn.PoId, grn.StorageLocationId,
            grn.ReceivedBy, grn.ReceivedAt,
            grn.Status.ToString(),
            grn.DeliveryNoteRef,
            grn.Lines.Count,
            grn.CreatedAt))];
    }
}
