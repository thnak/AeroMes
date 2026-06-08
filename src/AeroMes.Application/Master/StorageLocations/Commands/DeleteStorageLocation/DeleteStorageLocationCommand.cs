using MediatR;

namespace AeroMes.Application.Master.StorageLocations.Commands.DeleteStorageLocation;

public record DeleteStorageLocationCommand(int Id) : IRequest;
