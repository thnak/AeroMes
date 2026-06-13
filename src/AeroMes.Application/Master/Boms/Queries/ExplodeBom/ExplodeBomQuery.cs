using LiteBus.Queries.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Boms.Queries.ExplodeBom;

public record ExplodeBomQuery(string ProductCode, decimal Quantity = 1m)
    : IQuery<QueryResult<IReadOnlyList<ExplodedBomLineDto>>>;

public record ExplodedBomLineDto(
    int Level,
    string ParentCode,
    string ComponentCode,
    string? ComponentName,
    decimal RequiredQtyPerParent,
    decimal TotalRequiredQty,
    string UoMCode,
    decimal ScrapFactor,
    bool IsPhantom,
    bool HasChildBom);
