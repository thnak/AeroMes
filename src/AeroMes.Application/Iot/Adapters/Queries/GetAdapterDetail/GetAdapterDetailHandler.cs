using AeroMes.Application.Iot.Signals.Queries.GetSignals;
using AeroMes.Domain.Iot.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Iot.Adapters.Queries.GetAdapterDetail;

public class GetAdapterDetailHandler(IAdapterRepository repo) : IQueryHandler<GetAdapterDetailQuery, AdapterDetailDto?>
{
    public async Task<AdapterDetailDto?> HandleAsync(GetAdapterDetailQuery query, CancellationToken ct)
    {
        var adapter = await repo.GetByIdWithSignalsAsync(query.Id, ct);
        if (adapter is null) return null;

        var signals = adapter.Signals.Select(s => new SignalDto(
            s.SignalID, s.AdapterID, s.TagKey, s.DisplayName, s.SourceAddress,
            s.Scale, s.Offset, s.QualityMin, s.QualityMax, s.IsEnabled)).ToList();

        return new AdapterDetailDto(
            adapter.AdapterID, adapter.MachineCode, adapter.AdapterType, adapter.ConfigJson,
            adapter.Status, adapter.IsEnabled, adapter.LastSignalAt, adapter.WebhookApiKey, signals);
    }
}
