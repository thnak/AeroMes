using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Machines.Queries.GetMachines;

public record GetMachinesQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<MachineDto>>;

public record MachineDto(
    string MachineCode,
    string MachineName,
    int WorkCenterID,
    string? WorkCenterName,
    string? Brand,
    string? Model,
    string Status,
    bool IsActive,
    string? MachineCategory,
    decimal? TargetOeePct,
    decimal? TheoreticalCapacityPerHour,
    int PlannedDowntimeMinPerShift,
    decimal? HourlyCostRate,
    string? OpcUaNodeId,
    bool RequiresCertification,
    string? CertificationCode,
    byte MaxOperators,
    string MachineType,
    string? CustomAttributes,
    int? ClampingForceTons,
    string? SewingMachineClass);
