using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetPurchaseOrders;

public class GetPurchaseOrdersHandler(IPurchaseOrderRepository repo)
    : IQueryHandler<GetPurchaseOrdersQuery, IReadOnlyList<PurchaseOrderDto>>
{
    public async Task<IReadOnlyList<PurchaseOrderDto>> HandleAsync(GetPurchaseOrdersQuery q, CancellationToken ct)
    {
        PoStatus? status = null;
        if (q.Status.HasValue)
            status = q.Status;

        var pos = await repo.GetAllAsync(status, q.SupplierCode, ct);
        return [.. pos.Select(po => new PurchaseOrderDto(
            po.PoId, po.PoCode, po.SupplierCode,
            po.ExpectedDeliveryDate,
            po.Status.ToString(),
            po.Notes,
            po.Lines.Count,
            po.CreatedAt))];
    }
}
