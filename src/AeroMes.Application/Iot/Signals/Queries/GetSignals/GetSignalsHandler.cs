using AeroMes.Domain.Iot.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Iot.Signals.Queries.GetSignals;

public class GetSignalsHandler(ISignalMappingRepository repo) : IQueryHandler<GetSignalsQuery, IReadOnlyList<SignalDto>>
{
    public async Task<IReadOnlyList<SignalDto>> HandleAsync(GetSignalsQuery query, CancellationToken ct)
    {
        var signals = await repo.GetByAdapterAsync(query.AdapterId, ct);
        return signals.Select(s => new SignalDto(
            s.SignalID, s.AdapterID, s.TagKey, s.DisplayName, s.SourceAddress,
            s.Scale, s.Offset, s.QualityMin, s.QualityMax, s.IsEnabled)).ToList();
    }
}
