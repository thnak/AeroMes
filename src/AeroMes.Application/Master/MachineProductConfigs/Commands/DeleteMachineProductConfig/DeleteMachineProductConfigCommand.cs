using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.MachineProductConfigs.Commands.DeleteMachineProductConfig;

public record DeleteMachineProductConfigCommand(string MachineCode, string ProductCode) : ICommand;
