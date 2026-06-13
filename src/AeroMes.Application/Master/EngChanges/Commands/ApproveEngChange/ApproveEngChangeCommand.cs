using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.EngChanges.Commands.ApproveEngChange;

public record ApproveEngChangeCommand(string EcNumber, string? ApprovedBy) : ICommand<ValidationResult<Unit>>;
