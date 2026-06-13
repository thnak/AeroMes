using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetQualityReport;

public record GetQualityReportQuery(
    DateTime From,
    DateTime To,
    string? DefectCategory) : IQuery<QualityReportDto>;

public record QualityReportDto(
    DateTime From,
    DateTime To,
    int TotalDefects,
    IReadOnlyList<QualityReportRowDto> Rows);

public record QualityReportRowDto(
    string DefectCode,
    string DefectName,
    string? Category,
    int TotalQuantity,
    double Percentage);
