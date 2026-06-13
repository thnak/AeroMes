using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.StorageLocations.Commands.UpdateStorageLocation;

public record UpdateStorageLocationCommand(
    int Id,
    string Name,
    LocationType LocationType,
    int? WorkCenterId) : ICommand<ValidationResult<Unit>>;
