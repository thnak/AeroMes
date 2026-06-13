using LiteBus.Queries.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Auth.Sessions.Queries.GetUserSessions;

public record GetUserSessionsQuery(string UserId) : IQuery<QueryResult<IReadOnlyList<SessionDto>>>;
