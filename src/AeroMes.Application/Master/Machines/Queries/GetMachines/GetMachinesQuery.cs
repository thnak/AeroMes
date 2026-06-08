using MediatR;

namespace AeroMes.Application.Master.Machines.Queries.GetMachines;

public record GetMachinesQuery(bool ActiveOnly = true) : IRequest<IReadOnlyList<MachineDto>>;

public record MachineDto(
    string MachineCode,
    string MachineName,
    int WorkCenterID,
    string? WorkCenterName,
    string? Brand,
    string? Model,
    string Status,
    bool IsActive);
