using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetFactoryWarehouseReceipts;

public class GetFactoryWarehouseReceiptsHandler(IFactoryWarehouseReceiptRepository repo)
    : IQueryHandler<GetFactoryWarehouseReceiptsQuery, IReadOnlyList<FactoryWarehouseReceiptSummaryDto>>
{
    public async Task<IReadOnlyList<FactoryWarehouseReceiptSummaryDto>> HandleAsync(
        GetFactoryWarehouseReceiptsQuery query, CancellationToken ct)
    {
        var receipts = await repo.GetAllAsync(query.ReceiptType, query.Status, ct);

        return [.. receipts.Select(r => new FactoryWarehouseReceiptSummaryDto(
            r.ReceiptId,
            r.VoucherNumber,
            r.ReceiptType,
            r.Status,
            r.ReferenceRequestId,
            r.SupplierOrTransferringUnit,
            r.Lines.Count,
            r.CreatedAt,
            r.CreatedBy))];
    }
}
