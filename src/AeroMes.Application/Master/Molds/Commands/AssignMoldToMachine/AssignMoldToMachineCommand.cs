using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.AssignMoldToMachine;

public record AssignMoldToMachineCommand(
    string MoldCode,
    string MachineCode,
    string? UpdatedBy) : ICommand;
