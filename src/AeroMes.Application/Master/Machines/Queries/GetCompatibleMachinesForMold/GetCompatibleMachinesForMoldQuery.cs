using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Machines.Queries.GetCompatibleMachinesForMold;

public sealed record GetCompatibleMachinesForMoldQuery(int MinClampingForceTons)
    : IQuery<IReadOnlyList<CompatibleMachineDto>>;

public sealed record CompatibleMachineDto(
    string MachineCode,
    string MachineName,
    string MachineType,
    int WorkCenterID,
    string? WorkCenterName,
    string Status,
    int ClampingForceTons);
