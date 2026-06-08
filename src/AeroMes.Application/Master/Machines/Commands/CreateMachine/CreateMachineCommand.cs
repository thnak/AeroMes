using MediatR;

namespace AeroMes.Application.Master.Machines.Commands.CreateMachine;

public record CreateMachineCommand(
    string Code,
    string Name,
    int WorkCenterId,
    string? Brand,
    string? Model,
    string? CreatedBy) : IRequest<string>;
