using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Boms.Commands.ActivateBomVersion;

public record ActivateBomVersionCommand(
    string ProductCode,
    string Version,
    DateOnly? EffectiveFrom,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
