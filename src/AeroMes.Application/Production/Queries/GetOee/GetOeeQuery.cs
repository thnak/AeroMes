using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetOee;

public record GetOeeQuery(
    string MachineCode,
    DateTime ShiftStart,
    DateTime ShiftEnd,
    double DesignedCycleTimeSeconds) : IQuery<OeeResult>;

public record OeeResult(
    string MachineCode,
    double TotalPlannedMinutes,
    double DowntimeMinutes,
    int TotalProduced,
    int OkCount,
    int NgCount,
    double AvailabilityPercent,
    double PerformancePercent,
    double QualityPercent,
    double OeePercent);
