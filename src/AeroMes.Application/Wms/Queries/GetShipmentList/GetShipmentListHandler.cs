using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetShipmentList;

public class GetShipmentListHandler(IShipmentOrderRepository shipmentRepo)
    : IQueryHandler<GetShipmentListQuery, IReadOnlyList<ShipmentSummaryDto>>
{
    public async Task<IReadOnlyList<ShipmentSummaryDto>> HandleAsync(
        GetShipmentListQuery query, CancellationToken ct)
    {
        var shipments = await shipmentRepo.GetAllAsync(query.Status, ct);
        return shipments.Select(s => new ShipmentSummaryDto(
            s.ShipmentId, s.ShipmentCode, s.SoId, s.CustomerName,
            s.RequestedShipDate, s.Status, s.CarrierName, s.TrackingNumber,
            s.Lines.Count, s.CreatedAt)).ToList();
    }
}
