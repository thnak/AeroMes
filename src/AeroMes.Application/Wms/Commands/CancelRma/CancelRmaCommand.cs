using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CancelRma;

public record CancelRmaCommand(int RmaId, string? CancelledBy) : ICommand<ValidationResult<Unit>>;
