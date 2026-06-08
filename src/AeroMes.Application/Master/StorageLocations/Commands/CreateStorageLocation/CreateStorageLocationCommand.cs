using AeroMes.Domain.Master;
using MediatR;

namespace AeroMes.Application.Master.StorageLocations.Commands.CreateStorageLocation;

public record CreateStorageLocationCommand(
    string Code,
    string Name,
    LocationType LocationType,
    int? WorkCenterId) : IRequest<int>;
