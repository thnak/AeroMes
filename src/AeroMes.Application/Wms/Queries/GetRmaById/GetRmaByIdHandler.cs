using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetRmaById;

public class GetRmaByIdHandler(IRmaRepository rmaRepo)
    : IQueryHandler<GetRmaByIdQuery, RmaDetailDto?>
{
    public async Task<RmaDetailDto?> HandleAsync(GetRmaByIdQuery query, CancellationToken ct)
    {
        var rma = await rmaRepo.GetByIdWithLinesAsync(query.RmaId, ct);
        if (rma is null) return null;

        return new RmaDetailDto(
            rma.RmaId,
            rma.RmaCode,
            rma.ReturnDirection,
            rma.SourceDocumentType,
            rma.SourceDocumentId,
            rma.ReturnReason,
            rma.Status,
            rma.AuthorizedBy,
            rma.AuthorizedAt,
            rma.CreatedBy,
            rma.CreatedAt,
            rma.Lines.Select(l => new RmaLineDto(
                l.RmaLineId,
                l.ProductCode,
                l.LotNumber,
                l.ReturnQty,
                l.ReceivedQty,
                l.Disposition,
                l.NcrId,
                l.StockMovementId)).ToList());
    }
}
