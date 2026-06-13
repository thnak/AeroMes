using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.SaveErpSettings;

public record SaveErpSettingsCommand(
    bool ErpEnabled,
    string? ErpBaseUrl,
    string? ErpApiKey,
    int ErpSyncIntervalMinutes) : ICommand<ValidationResult<Unit>>;
