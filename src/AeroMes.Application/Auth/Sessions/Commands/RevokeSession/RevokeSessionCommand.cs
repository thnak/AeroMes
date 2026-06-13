using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Auth.Sessions.Commands.RevokeSession;

public record RevokeSessionCommand(string UserId, long TokenId) : ICommand<ValidationResult<Unit>>;
