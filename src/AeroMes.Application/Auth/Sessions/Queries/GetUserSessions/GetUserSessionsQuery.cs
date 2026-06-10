using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Auth.Sessions.Queries.GetUserSessions;

public record GetUserSessionsQuery(string UserId) : IQuery<IReadOnlyList<SessionDto>>;
