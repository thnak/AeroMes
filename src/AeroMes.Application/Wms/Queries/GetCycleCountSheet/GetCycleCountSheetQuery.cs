using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetCycleCountSheet;

public record GetCycleCountSheetQuery(int PlanId)
    : IQuery<IReadOnlyList<CycleCountSheetLineDto>?>;

public record CycleCountSheetLineDto(
    long LineId,
    int BinId,
    string ProductCode,
    string LotNumber,
    decimal? CountedQty,
    CycleCountLineStatus Status);
