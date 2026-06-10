using MediatR;

namespace AeroMes.Application.Auth.Sessions.Commands.RevokeAllSessions;

public record RevokeAllSessionsCommand(string UserId) : IRequest;
