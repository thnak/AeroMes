using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetGrnDetail;

public class GetGrnDetailHandler(IGoodsReceiptNoteRepository repo)
    : IQueryHandler<GetGrnDetailQuery, GrnDetailDto>
{
    public async Task<GrnDetailDto> HandleAsync(GetGrnDetailQuery q, CancellationToken ct)
    {
        var grn = await repo.GetByIdWithLinesAsync(q.GrnId, ct)
            ?? throw new EntityNotFoundException("GoodsReceiptNote", q.GrnId);

        return new GrnDetailDto(
            grn.GrnId, grn.GrnCode, grn.PoId, grn.StorageLocationId,
            grn.ReceivedBy, grn.ReceivedAt,
            grn.Status.ToString(),
            grn.DeliveryNoteRef, grn.Notes, grn.CreatedAt,
            [.. grn.Lines.Select(l => new GrnLineDetailDto(
                l.GrnLineId, l.PoLineId, l.ProductCode, l.LotNumber,
                l.ReceivedQty, l.ManufacturedDate, l.ExpiryDate,
                l.GrossWeightKg, l.QcStatus.ToString(), l.DestinationBinId))]);
    }
}
