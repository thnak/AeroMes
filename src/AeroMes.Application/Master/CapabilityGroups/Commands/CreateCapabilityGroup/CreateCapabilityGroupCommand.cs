using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.CapabilityGroups.Commands.CreateCapabilityGroup;

public record CreateCapabilityGroupCommand(
    string Code,
    string Name,
    string? Description) : ICommand<ValidationResult<string>>;
