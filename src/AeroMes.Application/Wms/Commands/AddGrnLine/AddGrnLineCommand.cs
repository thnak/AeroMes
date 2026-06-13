using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.AddGrnLine;

public record AddGrnLineCommand(
    int GrnId,
    int? PoLineId,
    string ProductCode,
    string LotNumber,
    decimal ReceivedQty,
    DateOnly? ManufacturedDate,
    DateOnly? ExpiryDate,
    decimal? GrossWeightKg,
    int? DestinationBinId,
    string? CreatedBy
) : ICommand<ValidationResult<GrnLineAddedResult>>;

public record GrnLineAddedResult(int GrnLineId);
