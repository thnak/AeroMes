using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.MachineProductConfigs.Commands.DeleteMachineProductConfig;

public record DeleteMachineProductConfigCommand(string MachineCode, string ProductCode) : ICommand<ValidationResult<Unit>>;
