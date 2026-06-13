using AeroMes.Domain.Iot.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Iot.Adapters.Queries.GetAdapters;

public class GetAdaptersHandler(IAdapterRepository repo) : IQueryHandler<GetAdaptersQuery, IReadOnlyList<AdapterDto>>
{
    public async Task<IReadOnlyList<AdapterDto>> HandleAsync(GetAdaptersQuery query, CancellationToken ct)
    {
        var adapters = await repo.GetByMachineAsync(query.MachineCode, ct);
        return adapters.Select(a => new AdapterDto(
            a.AdapterID, a.MachineCode, a.AdapterType, a.Status,
            a.IsEnabled, a.LastSignalAt, a.Signals.Count, a.WebhookApiKey)).ToList();
    }
}
