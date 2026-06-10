using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Auth.Sessions.Commands.RevokeSession;

public record RevokeSessionCommand(string UserId, long TokenId) : ICommand;
