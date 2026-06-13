using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.AddRmaLine;

public record AddRmaLineCommand(
    int RmaId,
    string ProductCode,
    string LotNumber,
    decimal ReturnQty,
    string? CreatedBy) : ICommand<ValidationResult<RmaLineAddedResult>>;

public record RmaLineAddedResult(int RmaLineId);
