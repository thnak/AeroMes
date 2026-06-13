using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CloseRma;

public record CloseRmaCommand(int RmaId, string? ClosedBy) : ICommand<ValidationResult<Unit>>;
