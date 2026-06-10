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
    bool IsActive);
