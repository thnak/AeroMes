using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Iot.Signals.Queries.GetSignals;

public record GetSignalsQuery(int AdapterId) : IQuery<IReadOnlyList<SignalDto>>;
