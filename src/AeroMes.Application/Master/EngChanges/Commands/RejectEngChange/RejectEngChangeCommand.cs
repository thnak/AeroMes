using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.EngChanges.Commands.RejectEngChange;

public record RejectEngChangeCommand(string EcNumber, string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
