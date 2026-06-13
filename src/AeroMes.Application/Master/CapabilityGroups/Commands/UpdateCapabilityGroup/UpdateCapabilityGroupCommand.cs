using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.CapabilityGroups.Commands.UpdateCapabilityGroup;

public record UpdateCapabilityGroupCommand(
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    string? UpdatedBy = null) : ICommand<ValidationResult<Unit>>;
