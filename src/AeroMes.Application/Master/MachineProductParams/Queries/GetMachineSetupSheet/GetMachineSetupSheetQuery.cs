using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.MachineProductParams.Queries.GetMachineSetupSheet;

public record GetMachineSetupSheetQuery(string MachineCode, string? ProductCode = null)
    : IQuery<IReadOnlyList<MachineProductParamDto>>;

public record MachineProductParamDto(
    string MachineCode,
    string ProductCode,
    string ParamName,
    string? Unit,
    decimal? NominalValue,
    decimal? MinValue,
    decimal? MaxValue,
    bool IsControlParam,
    int DisplayOrder);
