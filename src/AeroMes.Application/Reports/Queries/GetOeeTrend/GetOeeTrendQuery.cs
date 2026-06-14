using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetOeeTrend;

public enum OeeTrendGranularity { Day, Week, Month }

public record GetOeeTrendQuery(
    string MachineCode,
    DateTime From,
    DateTime To,
    OeeTrendGranularity Granularity) : IQuery<OeeTrendDto>;

public record OeeTrendDto(
    string MachineCode,
    DateTime From,
    DateTime To,
    string Granularity,
    IReadOnlyList<OeeTrendPointDto> Points);

public record OeeTrendPointDto(
    string Label,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    int QtyOK,
    int QtyNG,
    double DowntimeMinutes,
    double AvailabilityPct,
    double QualityPct,
    double OeePct);
