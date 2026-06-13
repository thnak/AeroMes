using AeroMes.Application.Common;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DisposeRmaLine;

public record DisposeRmaLineCommand(
    int RmaId,
    int LineId,
    RmaDisposition Disposition,
    int DispositionLocationId,
    int? DispositionBinId,
    string? Notes,
    string? DisposedBy) : ICommand<ValidationResult<Unit>>;
