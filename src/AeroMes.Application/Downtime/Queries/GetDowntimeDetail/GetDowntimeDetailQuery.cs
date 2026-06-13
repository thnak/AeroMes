using LiteBus.Queries.Abstractions;
using AeroMes.Application.Downtime.Queries.GetDowntimeLogs;
using AeroMes.Application.Common;

namespace AeroMes.Application.Downtime.Queries.GetDowntimeDetail;

public record GetDowntimeDetailQuery(long Id) : IQuery<QueryResult<DowntimeLogDto>>;
