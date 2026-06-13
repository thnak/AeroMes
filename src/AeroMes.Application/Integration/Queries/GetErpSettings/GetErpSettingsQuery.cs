using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Integration.Queries.GetErpSettings;

public record GetErpSettingsQuery : IQuery<ErpSettingsDto>;

public record ErpSettingsDto(
    bool ErpEnabled,
    string? ErpBaseUrl,
    bool HasApiKey,
    int ErpSyncIntervalMinutes,
    DateTime? ErpLastSyncAt);
