using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.FabricRolls.Commands.RegisterFabricRoll;

public record RegisterFabricRollCommand(
    string RollBarcode,
    string FabricProductCode,
    string LotNumber,
    string ShadeCode,
    decimal GrossLengthMeters,
    decimal GrossWeightKg,
    decimal FabricWidthCm,
    string? SupplierCode,
    int? LocationID) : ICommand<ValidationResult<int>>;
