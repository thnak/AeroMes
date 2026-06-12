using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.EngChanges.Commands.ApproveEngChange;

public record ApproveEngChangeCommand(string EcNumber, string? ApprovedBy) : ICommand;
