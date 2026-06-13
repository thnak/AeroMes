using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.TestErpConnection;

public record TestErpConnectionCommand : ICommand<ValidationResult<bool>>;
