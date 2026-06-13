using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.MachineProductParams.Commands.UpsertMachineProductParam;

public record UpsertMachineProductParamCommand(
    string MachineCode,
    string ProductCode,
    string ParamName,
    string? Unit,
    decimal? NominalValue,
    decimal? MinValue,
    decimal? MaxValue,
    bool IsControlParam,
    int DisplayOrder) : ICommand<ValidationResult<Unit>>;
