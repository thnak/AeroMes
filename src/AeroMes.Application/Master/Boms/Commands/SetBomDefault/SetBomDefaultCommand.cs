using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Boms.Commands.SetBomDefault;

public record SetBomDefaultCommand(
    string ProductCode,
    string Version,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
