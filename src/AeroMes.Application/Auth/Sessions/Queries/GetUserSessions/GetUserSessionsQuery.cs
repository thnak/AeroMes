using MediatR;

namespace AeroMes.Application.Auth.Sessions.Queries.GetUserSessions;

public record GetUserSessionsQuery(string UserId) : IRequest<IReadOnlyList<SessionDto>>;
