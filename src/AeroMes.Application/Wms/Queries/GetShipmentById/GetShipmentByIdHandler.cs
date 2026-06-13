using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetShipmentById;

public class GetShipmentByIdHandler(IShipmentOrderRepository shipmentRepo)
    : IQueryHandler<GetShipmentByIdQuery, ShipmentDetailDto?>
{
    public async Task<ShipmentDetailDto?> HandleAsync(
        GetShipmentByIdQuery query, CancellationToken ct)
    {
        var s = await shipmentRepo.GetByIdWithLinesAsync(query.ShipmentId, ct);
        if (s is null) return null;

        return new ShipmentDetailDto(
            s.ShipmentId, s.ShipmentCode, s.SoId, s.CustomerName,
            s.RequestedShipDate, s.Status, s.CarrierName, s.TrackingNumber,
            s.CreatedAt,
            s.Lines.Select(l => new ShipmentLineDto(
                l.LineId, l.ProductCode, l.OrderedQty, l.PickedQty, l.PackedQty)).ToList());
    }
}
