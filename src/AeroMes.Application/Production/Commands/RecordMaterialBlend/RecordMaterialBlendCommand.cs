using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.RecordMaterialBlend;

public record RecordMaterialBlendCommand(
    long JobID,
    string ResinProductCode,
    string VirginLotNumber,
    decimal VirginQtyKg,
    string? RegrindLotNumber,
    decimal RegrindQtyKg
) : ICommand<ValidationResult<long>>;
