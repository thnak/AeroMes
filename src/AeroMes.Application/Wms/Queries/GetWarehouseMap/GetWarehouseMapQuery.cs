using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetWarehouseMap;

public record GetWarehouseMapQuery(int ZoneId) : IQuery<IReadOnlyList<ZoneMapDto>>;

public record ZoneMapDto(
    int ZoneId,
    string ZoneCode,
    string ZoneName,
    string ZoneType,
    IReadOnlyList<AisleMapDto> Aisles);

public record AisleMapDto(
    int AisleId,
    string AisleCode,
    int PickSequence,
    IReadOnlyList<RackMapDto> Racks);

public record RackMapDto(
    int RackId,
    string RackCode,
    decimal? MaxWeightKg,
    IReadOnlyList<BinMapDto> Bins);

public record BinMapDto(
    int BinId,
    string BinCode,
    string BinLevel,
    string BinType,
    bool IsActive,
    decimal? MaxQty,
    int StockLines);
