using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.AuthorizeRma;

public record AuthorizeRmaCommand(
    int RmaId,
    string? AuthorizedBy) : ICommand<ValidationResult<Unit>>;
