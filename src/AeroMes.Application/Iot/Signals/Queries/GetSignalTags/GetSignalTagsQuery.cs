using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Iot.Signals.Queries.GetSignalTags;

public record GetSignalTagsQuery(string? Category, string? DataType) : IQuery<IReadOnlyList<SignalTagDto>>;
