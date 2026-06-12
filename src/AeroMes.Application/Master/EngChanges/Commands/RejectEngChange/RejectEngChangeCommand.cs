using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.EngChanges.Commands.RejectEngChange;

public record RejectEngChangeCommand(string EcNumber, string? UpdatedBy) : ICommand;
