using MediatR;

namespace AeroMes.Application.Production.Queries.GetOee;

public record GetOeeQuery(
    int WorkCenterId,
    string MachineCode,
    DateTime ShiftStart,
    DateTime ShiftEnd,
    double DesignedCycleTimeSeconds
) : IRequest<OeeResult>;

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
    double OeePercent
);
