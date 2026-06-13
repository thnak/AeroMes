using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.IncrementMoldShotCount;

/// <summary>Called when a job completes. Computes shots = QtyOK / CavityCount and logs them.</summary>
public record IncrementMoldShotCountCommand(
    string MoldCode,
    long JobID,
    long QtyOK
) : ICommand<ValidationResult<Unit>>;
