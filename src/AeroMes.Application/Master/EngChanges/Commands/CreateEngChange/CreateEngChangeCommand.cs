using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.EngChanges.Commands.CreateEngChange;

public record CreateEngChangeCommand(
    string EcNumber,
    EcType EcType,
    string Title,
    string? Description,
    EcReason Reason,
    EcPriority Priority,
    DateOnly? TargetDate,
    string? AffectedProducts,
    string? RequestedBy) : ICommand<ValidationResult<string>>;
