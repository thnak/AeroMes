using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.ReceiveBundleAtStation;

public record ReceiveBundleAtStationCommand(
    string BundleBarcode,
    string OperationCode,
    int WorkCenterID,
    string OperatorID) : ICommand<ValidationResult<Unit>>;
