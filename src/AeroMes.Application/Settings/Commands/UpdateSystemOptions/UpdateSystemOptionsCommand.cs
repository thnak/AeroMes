using AeroMes.Domain.Settings;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Settings.Commands.UpdateSystemOptions;

public record UpdateSystemOptionsCommand(SystemOptions Options, string? UpdatedBy) : ICommand;
