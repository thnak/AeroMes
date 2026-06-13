using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.StorageLocations.Commands.CreateStorageLocation;

public record CreateStorageLocationCommand(
    string Code,
    string Name,
    LocationType LocationType,
    int? WorkCenterId) : ICommand<ValidationResult<int>>;
