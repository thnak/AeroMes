using MediatR;

namespace AeroMes.Application.Master.Machines.Commands.DeleteMachine;

public record DeleteMachineCommand(string Code) : IRequest;
