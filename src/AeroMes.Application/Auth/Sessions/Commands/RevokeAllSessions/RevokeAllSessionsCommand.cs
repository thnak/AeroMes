using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Auth.Sessions.Commands.RevokeAllSessions;

public record RevokeAllSessionsCommand(string UserId) : ICommand<ValidationResult<Unit>>;
