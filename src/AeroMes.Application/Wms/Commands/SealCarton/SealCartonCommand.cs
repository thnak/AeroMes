using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.SealCarton;

public record SealCartonCommand(
    int CartonId,
    decimal? GrossWeightKg,
    string? SealedBy) : ICommand<ValidationResult<Unit>>;
