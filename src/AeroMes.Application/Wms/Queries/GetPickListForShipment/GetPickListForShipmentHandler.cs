using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetPickListForShipment;

public class GetPickListForShipmentHandler(IPickListRepository pickListRepo)
    : IQueryHandler<GetPickListForShipmentQuery, PickListDetailDto?>
{
    public async Task<PickListDetailDto?> HandleAsync(
        GetPickListForShipmentQuery query, CancellationToken ct)
    {
        var pl = await pickListRepo.GetByShipmentIdAsync(query.ShipmentId, ct);
        if (pl is null) return null;

        return new PickListDetailDto(
            pl.PickListId, pl.ShipmentId, pl.AssignedTo, pl.Status, pl.CompletedAt,
            pl.Lines.OrderBy(l => l.PickSequence).Select(l => new PickListLineDto(
                l.PickLineId, l.ShipmentLineId, l.ProductCode, l.LotNumber,
                l.LocationId, l.BinId, l.RequiredQty, l.PickedQty, l.IsConfirmed, l.PickSequence
            )).ToList());
    }
}
