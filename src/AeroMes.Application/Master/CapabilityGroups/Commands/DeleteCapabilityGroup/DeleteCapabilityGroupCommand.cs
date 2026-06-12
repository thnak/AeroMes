using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.CapabilityGroups.Commands.DeleteCapabilityGroup;

public record DeleteCapabilityGroupCommand(string Code, string? DeletedBy = null) : ICommand;
