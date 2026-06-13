using AeroMes.Domain.Production.Repositories;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetWarehouseMap;

public class GetWarehouseMapHandler(
    IWarehouseZoneRepository zoneRepo,
    IAisleRepository aisleRepo,
    IRackRepository rackRepo,
    IBinRepository binRepo,
    IInventoryStockRepository stockRepo)
    : IQueryHandler<GetWarehouseMapQuery, IReadOnlyList<ZoneMapDto>>
{
    public async Task<IReadOnlyList<ZoneMapDto>> HandleAsync(GetWarehouseMapQuery query, CancellationToken ct)
    {
        var zone = await zoneRepo.GetByIdAsync(query.ZoneId, ct);
        if (zone is null) return [];

        var aisles = await aisleRepo.GetByZoneAsync(zone.ZoneId, ct);
        var aisleMaps = new List<AisleMapDto>();

        foreach (var aisle in aisles.OrderBy(a => a.PickSequence))
        {
            var racks = await rackRepo.GetByAisleAsync(aisle.AisleId, ct);
            var rackMaps = new List<RackMapDto>();

            foreach (var rack in racks.OrderBy(r => r.RackCode))
            {
                var bins = await binRepo.GetByRackAsync(rack.RackId, false, ct);
                var binMaps = new List<BinMapDto>();

                foreach (var bin in bins.OrderBy(b => b.BinLevel).ThenBy(b => b.BinCode))
                {
                    var stockLines = await stockRepo.CountByBinAsync(bin.BinId, ct);
                    binMaps.Add(new BinMapDto(
                        bin.BinId, bin.BinCode, bin.BinLevel,
                        bin.BinType.ToString(), bin.IsActive, bin.MaxQty, stockLines));
                }

                rackMaps.Add(new RackMapDto(rack.RackId, rack.RackCode, rack.MaxWeightKg, binMaps));
            }

            aisleMaps.Add(new AisleMapDto(aisle.AisleId, aisle.AisleCode, aisle.PickSequence, rackMaps));
        }

        return [new ZoneMapDto(zone.ZoneId, zone.ZoneCode, zone.ZoneName, zone.ZoneType.ToString(), aisleMaps)];
    }
}
