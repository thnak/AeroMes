using AeroMes.Application.Common;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Statistics.Queries.GetProductionStatisticsSheetDetail;

public sealed record GetProductionStatisticsSheetDetailQuery(int SheetId)
    : IQuery<QueryResult<ProductionStatisticsSheetDetailDto>>;

public sealed record ProductionStatisticsSheetDetailDto(
    int SheetId,
    string SheetNumber,
    string SheetType,
    string Status,
    int? POID,
    int? MPOId,
    DateOnly ProductionDate,
    string? Notes,
    DateTime? CreatedAt,
    string? CreatedBy,
    IReadOnlyList<OutputLineDetailDto> OutputLines,
    IReadOnlyList<MaterialLineDetailDto> MaterialLines,
    IReadOnlyList<ByProductLineDetailDto> ByProductLines);

public sealed record OutputLineDetailDto(
    int LineId,
    string ProductCode,
    int PlannedQty,
    int QualifiedQty,
    int DefectiveQty,
    int? DefectCodeId);

public sealed record MaterialLineDetailDto(
    int LineId,
    string MaterialCode,
    decimal BomStandardQty,
    decimal ActualUsedQty,
    decimal Variance,
    string UoMCode,
    string? VarianceReason);

public sealed record ByProductLineDetailDto(
    int LineId,
    string ProductCode,
    int Qty,
    string UoMCode,
    string? WarehouseCode);
