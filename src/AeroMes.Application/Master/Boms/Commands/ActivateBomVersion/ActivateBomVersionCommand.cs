using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Boms.Commands.ActivateBomVersion;

public record ActivateBomVersionCommand(
    string ProductCode,
    string Version,
    DateOnly? EffectiveFrom,
    string? UpdatedBy) : ICommand;
