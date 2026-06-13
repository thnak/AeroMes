using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetProductionReport;

public record GetProductionReportQuery(
    DateTime From,
    DateTime To,
    string? WorkCenterCode,
    string? MachineCode) : IQuery<ProductionReportDto>;

public record ProductionReportDto(
    DateTime From,
    DateTime To,
    int TotalOK,
    int TotalNG,
    IReadOnlyList<ProductionReportRowDto> Rows);

public record ProductionReportRowDto(
    DateTime Date,
    string MachineCode,
    string? WorkCenterCode,
    string? WorkCenterName,
    int QtyOK,
    int QtyNG);
