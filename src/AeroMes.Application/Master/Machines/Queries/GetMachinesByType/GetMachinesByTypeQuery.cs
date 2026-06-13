using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Machines.Queries.GetMachinesByType;

public sealed record GetMachinesByTypeQuery(string MachineType, bool ActiveOnly = true)
    : IQuery<IReadOnlyList<MachinesByTypeDto>>;

public sealed record MachinesByTypeDto(
    string MachineCode,
    string MachineName,
    string MachineType,
    int WorkCenterID,
    string? WorkCenterName,
    string Status,
    bool IsActive,
    string? CustomAttributes,
    int? ClampingForceTons,
    string? SewingMachineClass);
