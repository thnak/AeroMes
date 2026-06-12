using LiteBus.Queries.Abstractions;
using AeroMes.Application.Downtime.Queries.GetDowntimeLogs;

namespace AeroMes.Application.Downtime.Queries.GetDowntimeDetail;

public record GetDowntimeDetailQuery(long Id) : IQuery<DowntimeLogDto>;
