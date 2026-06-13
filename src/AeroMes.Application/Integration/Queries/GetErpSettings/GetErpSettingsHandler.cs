using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Integration.Queries.GetErpSettings;

public class GetErpSettingsHandler(ISystemOptionsRepository repo)
    : IQueryHandler<GetErpSettingsQuery, ErpSettingsDto>
{
    public async Task<ErpSettingsDto> HandleAsync(GetErpSettingsQuery q, CancellationToken ct)
    {
        var options = await repo.GetAsync(ct);
        return new ErpSettingsDto(
            options.ErpEnabled,
            options.ErpBaseUrl,
            !string.IsNullOrEmpty(options.ErpApiKey),
            options.ErpSyncIntervalMinutes,
            options.ErpLastSyncAt);
    }
}
