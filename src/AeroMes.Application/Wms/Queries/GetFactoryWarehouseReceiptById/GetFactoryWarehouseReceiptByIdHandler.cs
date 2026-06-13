using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetFactoryWarehouseReceiptById;

public class GetFactoryWarehouseReceiptByIdHandler(IFactoryWarehouseReceiptRepository repo)
    : IQueryHandler<GetFactoryWarehouseReceiptByIdQuery, FactoryWarehouseReceiptDetailDto?>
{
    public async Task<FactoryWarehouseReceiptDetailDto?> HandleAsync(
        GetFactoryWarehouseReceiptByIdQuery query, CancellationToken ct)
    {
        var receipt = await repo.GetByIdWithLinesAsync(query.ReceiptId, ct);
        if (receipt is null) return null;

        return new FactoryWarehouseReceiptDetailDto(
            receipt.ReceiptId,
            receipt.VoucherNumber,
            receipt.ReceiptType,
            receipt.Status,
            receipt.ReferenceRequestId,
            receipt.SupplierOrTransferringUnit,
            receipt.Notes,
            [.. receipt.Lines.Select(l => new FactoryReceiptLineDto(
                l.LineId,
                l.ProductCode,
                l.UnitOfMeasure,
                l.Quantity,
                l.DestinationWarehouseId,
                l.SpecificationCode))],
            receipt.CreatedAt,
            receipt.CreatedBy,
            receipt.UpdatedAt,
            receipt.UpdatedBy);
    }
}
