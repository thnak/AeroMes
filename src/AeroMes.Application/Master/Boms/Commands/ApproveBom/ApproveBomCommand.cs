using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Boms.Commands.ApproveBom;

public record ApproveBomCommand(
    string ProductCode,
    string Version,
    string? ApprovedBy) : ICommand;
