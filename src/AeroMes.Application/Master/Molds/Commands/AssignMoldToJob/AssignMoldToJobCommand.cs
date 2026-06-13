using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.AssignMoldToJob;

public record AssignMoldToJobCommand(
    string MoldCode,
    string MachineCode,
    int WOID,
    long JobID,
    string AssignedBy
) : ICommand<ValidationResult<Unit>>;
