using AeroMes.Domain.Production;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Statistics.Queries.GetProductionStatisticsSheets;

public sealed record GetProductionStatisticsSheetsQuery(
    int? POID,
    int? MPOId,
    StatisticsSheetType? SheetType,
    StatisticsSheetStatus? Status,
    DateOnly? From,
    DateOnly? To) : IQuery<IReadOnlyList<ProductionStatisticsSheetSummaryDto>>;

public sealed record ProductionStatisticsSheetSummaryDto(
    int SheetId,
    string SheetNumber,
    string SheetType,
    string Status,
    int? POID,
    int? MPOId,
    DateOnly ProductionDate,
    int OutputLineCount,
    int TotalQualifiedQty,
    int TotalDefectiveQty,
    DateTime? CreatedAt,
    string? CreatedBy);
