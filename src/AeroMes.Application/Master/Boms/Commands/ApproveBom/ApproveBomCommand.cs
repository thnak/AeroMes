using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Boms.Commands.ApproveBom;

public record ApproveBomCommand(
    string ProductCode,
    string Version,
    string? ApprovedBy) : ICommand<ValidationResult<Unit>>;
