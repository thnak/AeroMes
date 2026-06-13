using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.MachineProductParams.Commands.DeleteMachineProductParam;

public record DeleteMachineProductParamCommand(
    string MachineCode,
    string ProductCode,
    string ParamName) : ICommand<ValidationResult<Unit>>;
