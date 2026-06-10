using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Auth.Sessions.Commands.RevokeAllSessions;

public record RevokeAllSessionsCommand(string UserId) : ICommand;
