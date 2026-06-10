using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Machines.Commands.DeleteMachine;

public record DeleteMachineCommand(string Code, string? DeletedBy = null) : ICommand;
