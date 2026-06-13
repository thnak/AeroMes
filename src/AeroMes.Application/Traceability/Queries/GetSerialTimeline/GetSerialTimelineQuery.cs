using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetSerialTimeline;

public record GetSerialTimelineQuery(string SerialNumber) : IQuery<IReadOnlyList<SerialEventDto>>;
