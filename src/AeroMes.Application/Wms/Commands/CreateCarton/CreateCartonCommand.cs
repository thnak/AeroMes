using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateCarton;

public record CreateCartonCommand(
    int ShipmentId,
    string? CreatedBy) : ICommand<ValidationResult<CartonCreatedResult>>;

public record CartonCreatedResult(int CartonId, string CartonCode);
