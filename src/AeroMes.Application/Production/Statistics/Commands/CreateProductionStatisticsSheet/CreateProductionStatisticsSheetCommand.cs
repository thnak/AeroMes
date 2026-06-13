using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Statistics.Commands.CreateProductionStatisticsSheet;

public sealed record OutputLineInput(
    string ProductCode,
    int PlannedQty,
    int QualifiedQty,
    int DefectiveQty,
    int? DefectCodeId);

public sealed record MaterialLineInput(
    string MaterialCode,
    decimal BomStandardQty,
    decimal ActualUsedQty,
    string UoMCode,
    string? VarianceReason);

public sealed record ByProductLineInput(
    string ProductCode,
    int Qty,
    string UoMCode,
    string? WarehouseCode);

public sealed record CreateProductionStatisticsSheetCommand(
    StatisticsSheetType SheetType,
    int? POID,
    int? MPOId,
    DateOnly ProductionDate,
    string? Notes,
    IReadOnlyList<OutputLineInput> OutputLines,
    IReadOnlyList<MaterialLineInput> MaterialLines,
    IReadOnlyList<ByProductLineInput> ByProductLines,
    string CreatedBy) : ICommand<ValidationResult<StatisticsSheetCreatedResult>>;

public sealed record StatisticsSheetCreatedResult(int SheetId, string SheetNumber);
