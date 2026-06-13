using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetBins;

public record GetBinsQuery(int RackId, bool ActiveOnly = true) : IQuery<IReadOnlyList<BinDto>>;

public record BinDto(
    int BinId,
    int RackId,
    string BinCode,
    string BinLevel,
    decimal? MaxQty,
    string BinType,
    bool IsActive);
