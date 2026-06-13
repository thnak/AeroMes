using AeroMes.Application.Common;
using AeroMes.Application.Traceability.Services;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.CommissionSerialUnits;

public record LotConsumptionDto(
    string ComponentLotNumber,
    string ComponentProductCode,
    decimal? QuantityUsed,
    string? UoM,
    int? RoutingStepID);

public record CommissionSerialUnitsCommand(
    int WorkOrderID,
    string LotNumber,
    string ProductCode,
    int Quantity,
    SerialStrategy SerialStrategy,
    DateOnly ProductionDate,
    DateOnly? ExpiryDate,
    string? GTIN,
    IReadOnlyList<LotConsumptionDto> ComponentLots
) : ICommand<ValidationResult<IReadOnlyList<string>>>;
