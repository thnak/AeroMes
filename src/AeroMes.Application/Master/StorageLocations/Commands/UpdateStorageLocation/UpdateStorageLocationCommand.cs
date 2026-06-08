using AeroMes.Domain.Master;
using MediatR;

namespace AeroMes.Application.Master.StorageLocations.Commands.UpdateStorageLocation;

public record UpdateStorageLocationCommand(
    int Id,
    string Name,
    LocationType LocationType,
    int? WorkCenterId) : IRequest;
